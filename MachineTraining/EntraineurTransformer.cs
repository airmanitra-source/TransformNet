namespace MachineLearning.ApiService.Models
{
    public class EntraineurTransformer
    {
        private MultiHeadTransformer _transformer;
        private EmbeddingLayer _embedding;
        private PositionalEncoding _positionalEncoding;
        private Vocabulaire _vocab;
        private double _learningRate = 0.01;
        private readonly int _tailleVocab;

        public EntraineurTransformer(MultiHeadTransformer transformer, EmbeddingLayer embedding, PositionalEncoding pe, Vocabulaire vocab)
        {
            _transformer = transformer;
            _embedding = embedding;
            _positionalEncoding = pe;
            _vocab = vocab;
            _tailleVocab = vocab.Compter();
        }

        public void Train(string[] mots, double[] cibles, int nbEpochs = 5000)
        {
            var tailleCible = cibles.Length;
            if (mots.Length == 0 || tailleCible == 0)
                throw new ArgumentException("Mots et cibles ne peuvent pas être vides");

            if (mots.Length != tailleCible)
                throw new ArgumentException($"Mismatch : mots ({mots.Length}) et cibles ({tailleCible}) doivent avoir la même longueur");

            bool print_initial_pe = false;
            // 📝 Affichage intelligent des tokens initiaux
            AfficherTokensInitiaux(mots);

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
                    double[] sortieBlock = _transformer.Executer(sequence[i], sequence, i);
                    double[] scoresVocab = _transformer.ObtenirScoresSortie();

                    // 2. Calculer l'erreur (cross-entropy)
                    int cibleIndex = (int)cibles[i];
                    erreurTotale += GetCrossEntropy(scoresVocab, cibleIndex);

                    // 3. Backward Pass
                    double[] gradientScores = GetGradientCrossEntropy(scoresVocab, _tailleVocab, tailleCible, cibleIndex);

                    _transformer.OutputLayer.AjouterGradient(sortieBlock, gradientScores);

                    double[] gradientBlock = _transformer.OutputLayer.CalculerGradientEntree(gradientScores);

                    double[] gradientEntree = _transformer.CalculerGradients(gradientBlock);

                    // Accumuler gradient pour l'embedding du mot courant
                    _embedding.AjouterGradient(motIds[i], gradientEntree);
                }

                // Mettre à jour TOUS les poids en fin d'epoch
                _embedding.MettreAJourPoids(_learningRate);
                _transformer.OutputLayer.UpdateWeights(_learningRate);
                _transformer.MettreAJourPoids(_learningRate);

                if (epoch % 200 == 0)
                    Console.WriteLine($"Epoch {epoch} : Erreur moyenne (cross-entropy) = {erreurTotale / tailleCible:F4}");
            }
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

        private void AfficherTokensInitiaux(string[] mots)
        {
            Console.WriteLine("\n" + new string('=', 100));
            Console.WriteLine($"🔤 Embeddings des Tokens Initiaux");
            Console.WriteLine($"   Nombre de tokens: {mots.Length}");
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
