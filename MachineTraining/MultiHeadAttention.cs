namespace MachineLearning.ApiService.Models
{
    public class MultiHeadAttention
    {
        private MaskedAttention[] _heads;
        private int _nbHeads;
        private int _headDim;
        private int _dimension;
        
        // État sauvegardé pour backprop
        private double[][] _derniereSequence;
        private int _dernierIndexMot;

        public MultiHeadAttention(int dimension, int nbHeads)
        {
            if (dimension % nbHeads != 0)
                throw new ArgumentException($"La dimension ({dimension}) doit être divisible par le nombre de têtes ({nbHeads})");

            _dimension = dimension;
            _nbHeads = nbHeads;
            _headDim = dimension / nbHeads; // Chaque tête traite une partie du vecteur
            _heads = new MaskedAttention[nbHeads];

            for (int i = 0; i < nbHeads; i++)
            {
                _heads[i] = new MaskedAttention(_headDim);
            }
        }

        public double[] Calculer(double[][] sequence, int indexMot)
        {
            // Valider l'index
            if (indexMot < 0 || indexMot >= sequence.Length)
                throw new ArgumentOutOfRangeException(nameof(indexMot));

            // Sauvegarder pour backprop
            _derniereSequence = sequence;
            _dernierIndexMot = indexMot;

            double[][] sortiesHeads = new double[_nbHeads][];

            // 1. Chaque tête travaille sur sa propre partie du vecteur (split)
            for (int h = 0; h < _nbHeads; h++)
            {
                double[][] sousSequence = ExtraireSousDimension(sequence, h);
                sortiesHeads[h] = _heads[h].CalculerAttention(sousSequence, indexMot);
            }

            // 2. On recolle les résultats de toutes les têtes (concatenation)
            return sortiesHeads.SelectMany(x => x).ToArray();
        }

        // ✅ Backpropagation multi-tête
        public double[][] CalculerGradientsSequence(double[] gradientSortie)
        {
            if (_derniereSequence == null)
                throw new InvalidOperationException("Aucun état sauvegardé. Appelez Calculer() d'abord.");

            double[][] gradientsSequence = new double[_derniereSequence.Length][];
            for (int i = 0; i < _derniereSequence.Length; i++)
                gradientsSequence[i] = new double[_dimension];

            // Backprop de chaque tête
            for (int h = 0; h < _nbHeads; h++)
            {
                // Extraire le gradient correspondant à cette tête
                double[] gradientHeadSortie = gradientSortie.Skip(h * _headDim).Take(_headDim).ToArray();

                // Backprop de la tête
                double[][] gradientsHeadSeq = _heads[h].CalculerGradientsSequence(gradientHeadSortie);

                // Ajouter au gradient total en repositionnant correctement
                for (int i = 0; i < gradientsHeadSeq.Length; i++)
                {
                    for (int d = 0; d < _headDim; d++)
                    {
                        gradientsSequence[i][h * _headDim + d] += gradientsHeadSeq[i][d];
                    }
                }
            }

            return gradientsSequence;
        }

        private double[][] ExtraireSousDimension(double[][] seq, int headIndex)
        {
            // Découpe le vecteur d'origine pour ne donner à la tête que sa portion
            return seq.Select(vecteur =>
                vecteur.Skip(headIndex * _headDim).Take(_headDim).ToArray()
            ).ToArray();
        }

        // Mettre à jour Q, K, V de toutes les têtes
        public void MettreAJourPoids(double tauxApprentissage)
        {
            foreach (var head in _heads)
                head.MettreAJourPoids(tauxApprentissage);
        }
    }

}
