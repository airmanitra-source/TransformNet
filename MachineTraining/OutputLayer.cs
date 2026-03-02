namespace MachineLearning.ApiService.Models
{
    public class OutputLayer
    {
        private double[][] _poids; // [Dimension x TailleVocabulaire]
        private double[][] _gradients; // Gradients pour l'apprentissage
        private double[] _biais;
        private double[] _gradientsBiais;

        public OutputLayer(int dimension, int tailleVocab)
        {
            _poids = Enumerable.Range(0, dimension)
                .Select(_ => Enumerable.Range(0, tailleVocab)
                    .Select(x => (Random.Shared.NextDouble() - 0.5) * (2.0 / Math.Sqrt(dimension)))
                    .ToArray())
                .ToArray();

            _gradients = Enumerable.Range(0, dimension)
                .Select(_ => new double[tailleVocab])
                .ToArray();

            _biais = Enumerable.Range(0, tailleVocab)
                .Select(_ => (Random.Shared.NextDouble() - 0.5) * 0.1)
                .ToArray();
            _gradientsBiais = new double[tailleVocab];
        }

        public double[] GenererScores(double[] vecteurInterne)
        {
            int tailleVocab = _poids[0].Length;
            double[] scores = new double[tailleVocab];

            // Multiplication matrice-vecteur
            for (int v = 0; v < tailleVocab; v++)
            {
                scores[v] = _biais[v];
                for (int d = 0; d < vecteurInterne.Length; d++)
                {
                    scores[v] += vecteurInterne[d] * _poids[d][v];
                }
            }
            return scores;
        }

        public void AjouterGradient(double[] vecteurInterne, double[] gradientSortie)
        {
            int tailleVocab = _poids[0].Length;
            
            // Gradient sur les poids
            for (int d = 0; d < vecteurInterne.Length; d++)
            {
                for (int v = 0; v < tailleVocab; v++)
                {
                    _gradients[d][v] += vecteurInterne[d] * gradientSortie[v];
                }
            }

            // Gradient sur les biais
            for (int v = 0; v < tailleVocab; v++)
            {
                _gradientsBiais[v] += gradientSortie[v];
            }
        }

        public void UpdateWeights(double tauxApprentissage)
        {
            int tailleVocab = _poids[0].Length;

            // Mettre à jour les poids (DESCENTE de gradient)
            for (int d = 0; d < _poids.Length; d++)
            {
                for (int v = 0; v < tailleVocab; v++)
                {
                    _poids[d][v] -= tauxApprentissage * _gradients[d][v];
                    _gradients[d][v] = 0; // Réinitialiser
                }
            }

            // Mettre à jour les biais
            for (int v = 0; v < tailleVocab; v++)
            {
                _biais[v] -= tauxApprentissage * _gradientsBiais[v];
                _gradientsBiais[v] = 0; // Réinitialiser
            }
        }

        // Rétropropager le gradient VERS le Transformer (vocabulaire → dimension)
        public double[] CalculerGradientEntree(double[] gradientSortie)
        {
            int dimension = _poids.Length;
            int tailleVocab = _poids[0].Length;
            double[] gradientEntree = new double[dimension];

            // dL/dx = W · dL/dy  (transposé de la multiplication avant)
            for (int d = 0; d < dimension; d++)
                for (int v = 0; v < tailleVocab; v++)
                    gradientEntree[d] += _poids[d][v] * gradientSortie[v];

            return gradientEntree;
        }
    }

}
