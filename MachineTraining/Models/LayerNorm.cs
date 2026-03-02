namespace MachineTraining.Models
{
    /// <summary>
    /// La Layer Normalization (LayerNorm) est le "stabilisateur" du Transformer. Sans elle, les valeurs explosent ou s'effondrent à chaque couche, rendant l'entraînement impossible (le fameux problème du Vanishing Gradient).
    /// </summary>
    public class LayerNorm
    {
        private double _gamma = 1.0; // Paramètre d'échelle (appris)
        private double _beta = 0.0;  // Paramètre de décalage (appris)
        private double _epsilon = 1e-5; // Petit nombre pour éviter la division par zéro
        private double _gradientGamma = 0.0; // Gradient pour gamma
        private double _gradientBeta = 0.0;  // Gradient pour beta

        public double[] Normaliser(double[] vecteur)
        {
            int n = vecteur.Length;
            double moyenne = vecteur.Average();

            // Calcul de la variance
            double variance = vecteur.Select(x => Math.Pow(x - moyenne, 2)).Average();
            double ecartType = Math.Sqrt(variance + _epsilon);

            // Transformation : (x - moy) / ecartType * gamma + beta
            return vecteur.Select(x => ((x - moyenne) / ecartType) * _gamma + _beta).ToArray();
        }

        public void AddGradient(double[] vecteur, double[] gradientSortie)
        {
            int n = vecteur.Length;
            double moyenne = vecteur.Average();
            double variance = vecteur.Select(x => Math.Pow(x - moyenne, 2)).Average();
            double ecartType = Math.Sqrt(variance + _epsilon);

            // Accumuler les gradients de gamma et beta
            for (int i = 0; i < n; i++)
            {
                double normalized = (vecteur[i] - moyenne) / ecartType;
                _gradientGamma += normalized * gradientSortie[i];
                _gradientBeta += gradientSortie[i];
            }
        }

        public void UpdateWeights(double tauxApprentissage)
        {
            _gamma -= tauxApprentissage * _gradientGamma;
            _beta -= tauxApprentissage * _gradientBeta;

            // Réinitialiser les gradients
            _gradientGamma = 0.0;
            _gradientBeta = 0.0;
        }
    }

}
