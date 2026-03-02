using MachineTraining.Models.Utils;

namespace MachineTraining.Models
{
    public class MultiHeadTransformer
    {
        // Propriétés publiques
        public MultiHeadAttention Attention { get; private set; }
        public MultiLayerPerceptron FeedForward { get; private set; }
        public OutputLayer OutputLayer { get; private set; }

        // Champs privés
        private LayerNorm _ln1 = new LayerNorm();
        private LayerNorm _ln2 = new LayerNorm();
        private EmbeddingLayer _embedding;
        private PositionalEncoding _positionalEncoding;
        private Vocabulary _vocab;
        private double _learningRate = 0.01;
        private readonly int _tailleVocab;

        // État sauvegardé pour backprop
        private double[] _derniereEntree;
        private double[][] _sequenceNorm1;
        private double[] _postAttention;
        private double[] _xNorm2;
        private double[] _ffnSortie;
        private double[] _sortieFinale;

        // ===== CONSTRUCTEUR =====
        public MultiHeadTransformer(int dimension, int nbHeads, int hiddenSize, int tailleVocab, 
                                    EmbeddingLayer embedding, PositionalEncoding pe, Vocabulary vocab)
        {
            Attention = new MultiHeadAttention(dimension, nbHeads);
            FeedForward = new MultiLayerPerceptron(dimension, hiddenSize);
            OutputLayer = new OutputLayer(dimension, tailleVocab);
            _tailleVocab = tailleVocab;
            
            // Initialiser les composants d'entraînement
            _embedding = embedding;
            _positionalEncoding = pe;
            _vocab = vocab;
        }

        // ===== MÉTHODES PUBLIQUES =====

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

        public double[] GetFinalScores()
        {
            if (_sortieFinale == null)
                throw new InvalidOperationException("Appelez Executer() d'abord");
            return OutputLayer.GenererScores(_sortieFinale);
        }

        public double[] BackPropagate(double[] gradientDimension)
        {
            if (_derniereEntree == null)
                throw new InvalidOperationException("Aucun état sauvegardé. Appelez Executer() d'abord.");

            int dim = _derniereEntree.Length;

            // --- Résidu 2 : gradient passe à la branche FFN ET au résidu ---
            double[] gradientFFN = FeedForward.BackwardPropagate(gradientDimension);
            _ln2.AddGradient(_postAttention, gradientFFN);

            // Gradient post-attention = résidu2 (copie directe) + gradient venant du FFN
            double[] gradientPostAttention = new double[dim];
            for (int i = 0; i < dim; i++)
            {
                gradientPostAttention[i] = gradientDimension[i] + gradientFFN[i];
            }

            // --- Résidu 1 : gradient passe à l'Attention ET au résidu ---
            Attention.CalculerGradientsSequence(gradientPostAttention);
            _ln1.AddGradient(_derniereEntree, gradientPostAttention);
            return ClipGradient(gradientPostAttention, 1.0);
        }

        private void MettreAJourPoids(double tauxApprentissage)
        {
            Attention.MettreAJourPoids(tauxApprentissage);
            FeedForward.UpdateWeights(tauxApprentissage);
            _ln1.UpdateWeights(tauxApprentissage);
            _ln2.UpdateWeights(tauxApprentissage);
        }

        public void Train(string[] mots, double[] cibles, int nbEpochs = 5000)
        {
            Console.WriteLine("Début de l'entrainement...");
            
            var tailleCible = cibles.Length;
            if (mots.Length == 0 || tailleCible == 0)
                throw new ArgumentException("Mots et cibles ne peuvent pas être vides");

            if (mots.Length != tailleCible)
                throw new ArgumentException($"Mismatch : mots ({mots.Length}) et cibles ({tailleCible}) doivent avoir la même longueur");

            bool print_initial_pe = false;
            PrintInitialTokens(mots);

            // Pré-calculer les IDs des mots (ne change pas)
            int[] motIds = mots.Select(m => _vocab.GetId(m)).ToArray();

            for (int epoch = 0; epoch < nbEpochs; epoch++)
            {
                double erreurTotale = 0;
                // Re-tokeniser depuis les embeddings MIS À JOUR à chaque epoch
                double[][] sequence = new double[mots.Length][];
                for (int i = 0; i < mots.Length; i++)
                {
                    sequence[i] = _embedding.GetCurrentVectorEmbeddingAtIndex(motIds[i]);
                }

                // Appliquer PE sur la séquence fraîche
                sequence = _positionalEncoding.GetPositionalEncoding(sequence);
                if (!print_initial_pe)
                {
                    // 🔍 Affichage intelligent du PE initial
                    _positionalEncoding.Print(tailleCible, $"Positional Encoding Initial (séquence de {tailleCible} mots)");
                    print_initial_pe = true;
                }

                for (int i = 0; i < tailleCible; i++)
                {
                    // 1. Forward Pass
                    double[] sortieBlock = Executer(sequence[i], sequence, i);
                    double[] scoresVocab = GetFinalScores();

                    // 2. Calculer l'erreur (cross-entropy)
                    int cibleIndex = (int)cibles[i];
                    erreurTotale += GetCrossEntropy(scoresVocab, cibleIndex);

                    // 3. Backward Pass
                    double[] gradientScores = GetGradientCrossEntropy(scoresVocab, _tailleVocab, tailleCible, cibleIndex);

                    OutputLayer.AjouterGradient(sortieBlock, gradientScores);

                    double[] gradientBlock = OutputLayer.CalculerGradientEntree(gradientScores);

                    double[] gradientEntree = BackPropagate(gradientBlock);

                    // Accumuler gradient pour l'embedding du mot courant
                    _embedding.AjouterGradient(motIds[i], gradientEntree);
                }

                // Mettre à jour TOUS les poids en fin d'epoch
                _embedding.MettreAJourPoids(_learningRate);
                OutputLayer.UpdateWeights(_learningRate);
                MettreAJourPoids(_learningRate);

                if (epoch % 200 == 0)
                    Console.WriteLine($"Epoch {epoch} : Erreur moyenne (cross-entropy) = {erreurTotale / tailleCible:F4}");
            }
            Console.WriteLine("Entraînement terminé.\n");
        }

        public void GenerateNextWords(string[] phrasesDebut, int nbMotsAGenerer = 3, double temperature = 0.8)
        {
            var generateur = new TextUtils();

            foreach (string debutPhrase in phrasesDebut)
            {
                Console.WriteLine($"\n{new string('=', 80)}");
                Console.WriteLine($"🎯 Tu tapes: \"{debutPhrase}\"");
                Console.WriteLine(new string('=', 80));
                Console.Write($"Réponse du LLM : {debutPhrase} ");

                string phraseActuelle = debutPhrase;

                for (int i = 0; i < nbMotsAGenerer; i++)
                {
                    // Tokeniser la phrase actuelle depuis les embeddings appris
                    double[][] seq = _vocab.Tokeniser(phraseActuelle, _embedding);
                    seq = _positionalEncoding.GetPositionalEncoding(seq);

                    // Passer le dernier token avec toute la séquence comme contexte
                    int pos = seq.Length - 1;
                    double[] sortie = Executer(seq[pos], seq, pos);
                    double[] scores = GetFinalScores();

                    // Choisir le prochain mot (température modérée)
                    int nextId = generateur.ChoisirIndex(scores, temperature: temperature);
                    string motPredi = _vocab.GetMot(nextId);

                    Console.Write(motPredi + " ");
                    phraseActuelle += " " + motPredi;
                }

                Console.WriteLine(); // Saut de ligne après chaque génération
            }

            Console.WriteLine("\n" + new string('=', 80));
        }

        // ===== MÉTHODES PRIVÉES =====

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

        private double[] GetGradientCrossEntropy(double[] scoresVocab, int tailleVocab, int tailleCible, int cibleIndex)
        {
            double maxScore = scoresVocab.Max();
            double[] exp = scoresVocab.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = exp.Sum();
            double[] probas = exp.Select(e => e / somme).ToArray();

            double[] gradients = new double[tailleVocab];
            for (int j = 0; j < gradients.Length; j++)
            {
                gradients[j] = probas[j];
                if (j == cibleIndex) gradients[j] -= 1.0;
                gradients[j] = gradients[j] * (1.0 / tailleCible);
            }
            return gradients;
        }

        private double GetCrossEntropy(double[] scoresVocab, int cibleIndex)
        {
            double maxScore = scoresVocab.Max();
            double[] exp = scoresVocab.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = exp.Sum();
            double probaCible = exp[cibleIndex] / somme;
            return -Math.Log(Math.Max(probaCible, 1e-10));
        }

        private void PrintInitialTokens(string[] mots)
        {
            Console.WriteLine("\n" + new string('=', 100));
            Console.WriteLine($" Embeddings des Tokens Initiaux");
            Console.WriteLine($"   # tokens: {mots.Length}");
            Console.WriteLine(new string('=', 100));

            int[] motIds = mots.Select(m => _vocab.GetId(m)).ToArray();
            int affichageTokens = Math.Min(8, mots.Length);

            // Afficher les premiers tokens
            for (int i = 0; i < affichageTokens; i++)
            {
                double[] embedding = _embedding.GetCurrentVectorEmbeddingAtIndex(motIds[i]);
                Console.WriteLine($"\n   Token {i}: '{mots[i]}' (ID: {motIds[i]})");
                Console.Write("   Embedding[");
                for (int j = 0; j < embedding.Length; j++)
                {
                    Console.Write($"{embedding[j],8:F4}");
                    if (j < embedding.Length - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Si la séquence est longue, afficher aussi le dernier token
            if (mots.Length > affichageTokens)
            {
                int lastIdx = mots.Length - 1;
                double[] embeddingDernier = _embedding.GetCurrentVectorEmbeddingAtIndex(motIds[lastIdx]);
                Console.WriteLine($"\n   Token {lastIdx} (dernier): '{mots[lastIdx]}' (ID: {motIds[lastIdx]})");
                Console.Write("   Embedding[");
                for (int j = 0; j < embeddingDernier.Length; j++)
                {
                    Console.Write($"{embeddingDernier[j],8:F4}");
                    if (j < embeddingDernier.Length - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Statistiques globales sur tous les embeddings
            Console.WriteLine("\n   📊 Statistiques sur l'ensemble des embeddings:");
            double[][] tousLesEmbeddings = motIds.Select(id => _embedding.GetCurrentVectorEmbeddingAtIndex(id)).ToArray();
            double minVal = tousLesEmbeddings.SelectMany(x => x).Min();
            double maxVal = tousLesEmbeddings.SelectMany(x => x).Max();
            double moyVal = tousLesEmbeddings.SelectMany(x => x).Average();
            double stdDev = Math.Sqrt(tousLesEmbeddings.SelectMany(x => x)
                .Average(x => Math.Pow(x - moyVal, 2)));

            Console.WriteLine($"   Min: {minVal:F6} | Max: {maxVal:F6}");
            Console.WriteLine($"   Moyenne: {moyVal:F6} | Écart-type: {stdDev:F6}");
            Console.WriteLine(new string('=', 80) + "\n");
        }
    }
}
