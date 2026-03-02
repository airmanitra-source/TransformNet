namespace MachineLearning.ApiService.Models
{
    public class MultiHeadTransformer
    {
        public MultiHeadAttention Attention { get; private set; }

        public MultiLayerPerceptron FeedForward { get; private set; }

        public OutputLayer OutputLayer { get; private set; }

        private LayerNorm _ln1 = new LayerNorm();
        private LayerNorm _ln2 = new LayerNorm();

        // État sauvegardé pour backprop
        private double[] _derniereEntree;
        private double[][] _sequenceNorm1;
        private double[] _postAttention;
        private double[] _xNorm2;
        private double[] _ffnSortie;
        private double[] _sortieFinale;

        public MultiHeadTransformer(int dimension, int nbHeads, int hiddenSize, int tailleVocab)
        {
            Attention = new MultiHeadAttention(dimension, nbHeads);
            FeedForward = new MultiLayerPerceptron(dimension, hiddenSize);
            OutputLayer = new OutputLayer(dimension, tailleVocab);
        }

        public double[] Executer(double[] x, double[][] sequence, int index)
        {
            _derniereEntree = (double[])x.Clone();

            // --- ÉTAPE 1 : Attention avec Résidu (Pre-Norm) ---
            // Normaliser TOUTE la séquence pour l'attention
            _sequenceNorm1 = new double[sequence.Length][];
            for (int i = 0; i < sequence.Length; i++)
                _sequenceNorm1[i] = _ln1.Normaliser(sequence[i]);

            double[] attention = Attention.Calculer(_sequenceNorm1, index);
            _postAttention = ResNetUtils.Ajouter(x, attention);

            // --- ÉTAPE 2 : FFN avec Résidu (Pre-Norm) ---
            _xNorm2 = _ln2.Normaliser(_postAttention);
            _ffnSortie = FeedForward.ForwardPropagate(_xNorm2);
            _sortieFinale = ResNetUtils.Ajouter(_postAttention, _ffnSortie);

            return _sortieFinale;
        }

        public double[] ObtenirScoresSortie()
        {
            if (_sortieFinale == null)
                throw new InvalidOperationException("Appelez Executer() d'abord");
            return OutputLayer.GenererScores(_sortieFinale);
        }

        // Backpropagation du bloc Transformer (reçoit gradient de taille 'dimension')
        public double[] CalculerGradients(double[] gradientDimension)
        {
            if (_derniereEntree == null)
                throw new InvalidOperationException("Aucun état sauvegardé. Appelez Executer() d'abord.");

            int dim = _derniereEntree.Length;

            // --- Résidu 2 : gradient passe à la branche FFN ET au résidu ---
            // Branche FFN : rétropropager à travers FFN puis LayerNorm2
            double[] gradientFFN = FeedForward.BackwardPropagate(gradientDimension);
            _ln2.AjouterGradient(_postAttention, gradientFFN);

            // Gradient post-attention = résidu2 (copie directe) + gradient venant du FFN
            double[] gradientPostAttention = new double[dim];
            for (int i = 0; i < dim; i++)
                gradientPostAttention[i] = gradientDimension[i] + gradientFFN[i];

            // --- Résidu 1 : gradient passe à l'Attention ET au résidu ---
            // Branche Attention : accumule les gradients Q,K,V en interne
            Attention.CalculerGradientsSequence(gradientPostAttention);
            _ln1.AjouterGradient(_derniereEntree, gradientPostAttention);
            // Le gradient pour l'entrée vient du chemin résiduel direct
            return ClipGradient(gradientPostAttention, 1.0);          
        }

        // Mettre à jour TOUS les poids du bloc (Q,K,V + FFN + LayerNorms)
        public void MettreAJourPoids(double tauxApprentissage)
        {
            Attention.MettreAJourPoids(tauxApprentissage);
            FeedForward.UpdateWeights(tauxApprentissage);
            _ln1.MettreAJourPoids(tauxApprentissage);
            _ln2.MettreAJourPoids(tauxApprentissage);
        }

        private double[] ClipGradient(double[] gradient, double maxNorm)
        {
            double norm = Math.Sqrt(gradient.Sum(g => g * g));
            if (norm > maxNorm)
            {
                double scale = maxNorm / norm;
                for (int i = 0; i < gradient.Length; i++)
                    gradient[i] *= scale;
            }
            return gradient;
        }
    }

    public class ResNetUtils
    {
        // Additionne l'entrée originale au résultat de la couche
        public static double[] Ajouter(double[] entree, double[] sortieCouche)
        {
            if (entree.Length != sortieCouche.Length)
                throw new ArgumentException("Les dimensions doivent correspondre !");

            double[] resultat = new double[entree.Length];
            for (int i = 0; i < entree.Length; i++)
                resultat[i] = entree[i] + sortieCouche[i];

            return resultat;
        }
    }

}
