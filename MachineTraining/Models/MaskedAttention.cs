namespace MachineTraining.Models
{
    public class MaskedAttention
    {
        // Champs privés
        private int _dimension;
        private const double MASQUE_VALUE = -1e9;
        
        // ✅ Matrices apprenables Q, K, V
        private double[][] _matriceQ;  // [dimension x dimension]
        private double[][] _matriceK;  // [dimension x dimension]
        private double[][] _matriceV;  // [dimension x dimension]
        
        private double[][] _gradientsQ;
        private double[][] _gradientsK;
        private double[][] _gradientsV;
        
        // État sauvegardé pour backprop
        private double[] _derniereScores;
        private double[] _derniersPoids;
        private double[][] _derniereSequence;
        private int _dernierIndex;
        
        private double[] _derniereQ;
        private double[][] _derniereK;
        private double[][] _derniereV;

        // ===== CONSTRUCTEUR =====
        public MaskedAttention(int dimension)
        {
            _dimension = dimension;
            
            // Initialiser Q, K, V avec Xavier initialization
            double limite = Math.Sqrt(6.0 / dimension);
            _matriceQ = InitialiserMatrice(dimension, dimension, limite);
            _matriceK = InitialiserMatrice(dimension, dimension, limite);
            _matriceV = InitialiserMatrice(dimension, dimension, limite);
            
            _gradientsQ = new double[dimension][];
            _gradientsK = new double[dimension][];
            _gradientsV = new double[dimension][];
            for (int i = 0; i < dimension; i++)
            {
                _gradientsQ[i] = new double[dimension];
                _gradientsK[i] = new double[dimension];
                _gradientsV[i] = new double[dimension];
            }
        }

        // ===== MÉTHODES PUBLIQUES =====

        public double[] CalculerAttention(double[][] sequence, int indexMotActuel)
        {
            int n = sequence.Length;
            
            // ✅ Projeter en Q, K, V
            _derniereQ = ProjetterVecteur(sequence[indexMotActuel], _matriceQ);
            _derniereK = new double[n][];
            _derniereV = new double[n][];
            
            for (int i = 0; i < n; i++)
            {
                _derniereK[i] = ProjetterVecteur(sequence[i], _matriceK);
                _derniereV[i] = ProjetterVecteur(sequence[i], _matriceV);
            }

            // ✅ Calculer scores avec Q·K^T
            double[] scores = new double[n];
            double scaling = Math.Sqrt(_dimension);

            for (int i = 0; i <= indexMotActuel; i++)
            {
                scores[i] = ProduitScalaire(_derniereQ, _derniereK[i]) / scaling;
            }

            for (int i = indexMotActuel + 1; i < n; i++)
            {
                scores[i] = MASQUE_VALUE;
            }

            double[] poids = SoftmaxStable(scores);

            // Sauvegarder pour backprop
            _derniereScores = scores;
            _derniersPoids = poids;
            _derniereSequence = sequence;
            _dernierIndex = indexMotActuel;

            // ✅ Appliquer attention à V
            return AppliquerPoids(_derniereV, poids);
        }

        public double[][] CalculerGradientsSequence(double[] gradientSortie)
        {
            if (_derniereSequence == null)
                throw new InvalidOperationException("Aucun état sauvegardé. Appelez CalculerAttention() d'abord.");

            int n = _derniereSequence.Length;
            double scaling = Math.Sqrt(_dimension);

            // 1. dL/dV_i = poids[i] * gradientSortie
            double[][] gradientV = new double[n][];
            for (int i = 0; i < n; i++)
            {
                gradientV[i] = new double[_dimension];
                for (int d = 0; d < _dimension; d++)
                    gradientV[i][d] = _derniersPoids[i] * gradientSortie[d];
            }

            // 2. dL/d(poids_i) = sum_d(gradientSortie[d] * V[i][d])
            double[] gradientPoids = new double[n];
            for (int i = 0; i < n; i++)
                for (int d = 0; d < _dimension; d++)
                    gradientPoids[i] += gradientSortie[d] * _derniereV[i][d];

            // 3. Gradient de softmax : dL/d(score_i)
            double[] gradientScores = new double[n];
            for (int i = 0; i <= _dernierIndex; i++)
                for (int j = 0; j <= _dernierIndex; j++)
                {
                    double delta = (i == j) ? 1.0 : 0.0;
                    gradientScores[i] += _derniersPoids[i] * (delta - _derniersPoids[j]) * gradientPoids[j];
                }

            // 4. Appliquer scaling
            for (int i = 0; i < n; i++)
                gradientScores[i] /= scaling;

            // 5. Accumuler gradients dans les matrices Q, K, V
            double[] gradientQ = new double[_dimension];
            for (int i = 0; i <= _dernierIndex; i++)
                for (int d = 0; d < _dimension; d++)
                    gradientQ[d] += gradientScores[i] * _derniereK[i][d];

            // dL/dWQ += outer(dL/dQ, x_query)
            for (int r = 0; r < _dimension; r++)
                for (int c = 0; c < _dimension; c++)
                    _gradientsQ[r][c] += gradientQ[r] * _derniereSequence[_dernierIndex][c];

            // dL/dK_i = gradientScores[i] * Q
            for (int i = 0; i <= _dernierIndex; i++)
                for (int r = 0; r < _dimension; r++)
                    for (int c = 0; c < _dimension; c++)
                        _gradientsK[r][c] += (gradientScores[i] * _derniereQ[r]) * _derniereSequence[i][c];

            // dL/dWV += outer(dL/dV_i, x_i)
            for (int i = 0; i < n; i++)
                for (int r = 0; r < _dimension; r++)
                    for (int c = 0; c < _dimension; c++)
                        _gradientsV[r][c] += gradientV[i][r] * _derniereSequence[i][c];

            // 6. Gradient par rapport à la séquence d'entrée
            double[][] gradientsSequence = new double[n][];
            for (int i = 0; i < n; i++)
            {
                gradientsSequence[i] = new double[_dimension];
                // Via V
                for (int d = 0; d < _dimension; d++)
                    for (int k = 0; k < _dimension; k++)
                        gradientsSequence[i][d] += _matriceV[k][d] * gradientV[i][k];

                // Via K
                if (i <= _dernierIndex)
                    for (int d = 0; d < _dimension; d++)
                        for (int k = 0; k < _dimension; k++)
                            gradientsSequence[i][d] += _matriceK[k][d] * gradientScores[i] * _derniereQ[k];
            }
            // Via Q
            for (int d = 0; d < _dimension; d++)
                for (int k = 0; k < _dimension; k++)
                    gradientsSequence[_dernierIndex][d] += _matriceQ[k][d] * gradientQ[k];

            return gradientsSequence;
        }

        public void MettreAJourPoids(double tauxApprentissage)
        {
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _dimension; j++)
                {
                    _matriceQ[i][j] -= tauxApprentissage * _gradientsQ[i][j];
                    _matriceK[i][j] -= tauxApprentissage * _gradientsK[i][j];
                    _matriceV[i][j] -= tauxApprentissage * _gradientsV[i][j];

                    _gradientsQ[i][j] = 0;
                    _gradientsK[i][j] = 0;
                    _gradientsV[i][j] = 0;
                }
            }
        }

        public double[][] ObtenirMatriceQ() => _matriceQ.Select(row => (double[])row.Clone()).ToArray();
        public double[][] ObtenirMatriceK() => _matriceK.Select(row => (double[])row.Clone()).ToArray();
        public double[][] ObtenirMatriceV() => _matriceV.Select(row => (double[])row.Clone()).ToArray();

        public void ChargerMatrices(double[][] Q, double[][] K, double[][] V)
        {
            _matriceQ = Q.Select(row => (double[])row.Clone()).ToArray();
            _matriceK = K.Select(row => (double[])row.Clone()).ToArray();
            _matriceV = V.Select(row => (double[])row.Clone()).ToArray();
        }

        // ===== MÉTHODES PRIVÉES =====

        private double[][] InitialiserMatrice(int rows, int cols, double limite)
        {
            double[][] matrice = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrice[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    matrice[i][j] = (Random.Shared.NextDouble() * 2 - 1) * limite;
                }
            }
            return matrice;
        }

        private double[] ProjetterVecteur(double[] vecteur, double[][] matrice)
        {
            double[] resultat = new double[_dimension];
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < vecteur.Length; j++)
                {
                    resultat[i] += vecteur[j] * matrice[i][j];
                }
            }
            return resultat;
        }

        private double[] AppliquerPoids(double[][] seq, double[] poids)
        {
            double[] sortie = new double[_dimension];
            for (int i = 0; i < seq.Length; i++)
            {
                if (poids[i] == 0 || double.IsNaN(poids[i])) continue;
                for (int d = 0; d < _dimension; d++) 
                    sortie[d] += poids[i] * seq[i][d];
            }
            return sortie;
        }

        private double ProduitScalaire(double[] a, double[] b) => a.Zip(b, (x, y) => x * y).Sum();

        private double[] SoftmaxStable(double[] entrees)
        {
            double maxVal = entrees.Where(x => x > MASQUE_VALUE).DefaultIfEmpty(0).Max();
            double[] exp = entrees.Select(e => e <= MASQUE_VALUE ? 0 : Math.Exp(e - maxVal)).ToArray();
            double somme = exp.Sum();
            
            if (somme == 0) return entrees.Select(x => 0.0).ToArray();
            
            return exp.Select(e => e / somme).ToArray();
        }
    }
}
