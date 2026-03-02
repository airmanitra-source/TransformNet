namespace MachineTraining.Models
{
    /// <summary>
    /// Couche Feed-Forward standard d'un Transformer : FFN(x) = W2 · ReLU(W1 · x + b1) + b2
    /// Transforme un vecteur de dimension 'dim' en passant par une couche cachée de taille 'hidden'.
    /// </summary>
    public class MultiLayerPerceptron
    {
        private double[][] _W1; // [hidden x dim]
        private double[] _b1;   // [hidden]
        private double[][] _W2; // [dim x hidden]
        private double[] _b2;   // [dim]

        private double[][] _gW1, _gW2;
        private double[] _gb1, _gb2;

        // État sauvegardé pour backprop
        private double[] _lastInput;
        private double[] _lastPreRelu;
        private double[] _lastHidden;

        public int Dimension { get; }
        public int HiddenSize { get; }

        public MultiLayerPerceptron(int dim, int hidden)
        {
            Dimension = dim;
            HiddenSize = hidden;

            // Initialisation He (adaptée à ReLU)
            double scaleW1 = Math.Sqrt(2.0 / dim);
            _W1 = new double[hidden][];
            _gW1 = new double[hidden][];
            _b1 = new double[hidden];
            _gb1 = new double[hidden];
            for (int h = 0; h < hidden; h++)
            {
                _W1[h] = new double[dim];
                _gW1[h] = new double[dim];
                for (int d = 0; d < dim; d++)
                    _W1[h][d] = (Random.Shared.NextDouble() - 0.5) * scaleW1;
            }

            double scaleW2 = Math.Sqrt(2.0 / hidden);
            _W2 = new double[dim][];
            _gW2 = new double[dim][];
            _b2 = new double[dim];
            _gb2 = new double[dim];
            for (int d = 0; d < dim; d++)
            {
                _W2[d] = new double[hidden];
                _gW2[d] = new double[hidden];
                for (int h = 0; h < hidden; h++)
                    _W2[d][h] = (Random.Shared.NextDouble() - 0.5) * scaleW2;
            }
        }

        public double[] ForwardPropagate(double[] x)
        {
            int dim = x.Length;
            int hidden = _W1.Length;
            _lastInput = (double[])x.Clone();

            // Couche 1 : hidden = ReLU(W1 · x + b1)
            _lastPreRelu = new double[hidden];
            _lastHidden = new double[hidden];
            for (int h = 0; h < hidden; h++)
            {
                double sum = _b1[h];
                for (int d = 0; d < dim; d++)
                    sum += _W1[h][d] * x[d];
                _lastPreRelu[h] = sum;
                _lastHidden[h] = Math.Max(0, sum); // ReLU
            }

            // Couche 2 : output = W2 · hidden + b2
            double[] output = new double[dim];
            for (int d = 0; d < dim; d++)
            {
                output[d] = _b2[d];
                for (int h = 0; h < hidden; h++)
                    output[d] += _W2[d][h] * _lastHidden[h];
            }

            return output;
        }

        public double[] BackwardPropagate(double[] gradOutput)
        {
            int dim = _lastInput.Length;
            int hidden = _W1.Length;

            // Gradient sur W2 et b2
            for (int d = 0; d < dim; d++)
            {
                _gb2[d] += gradOutput[d];
                for (int h = 0; h < hidden; h++)
                    _gW2[d][h] += gradOutput[d] * _lastHidden[h];
            }

            // Gradient sur hidden
            double[] gradHidden = new double[hidden];
            for (int h = 0; h < hidden; h++)
                for (int d = 0; d < dim; d++)
                    gradHidden[h] += _W2[d][h] * gradOutput[d];

            // Dérivée de ReLU
            for (int h = 0; h < hidden; h++)
                if (_lastPreRelu[h] <= 0) gradHidden[h] = 0;

            // Gradient sur W1 et b1
            for (int h = 0; h < hidden; h++)
            {
                _gb1[h] += gradHidden[h];
                for (int d = 0; d < dim; d++)
                    _gW1[h][d] += gradHidden[h] * _lastInput[d];
            }

            // Gradient sur l'entrée (pour rétropropager en amont)
            double[] gradInput = new double[dim];
            for (int d = 0; d < dim; d++)
                for (int h = 0; h < hidden; h++)
                    gradInput[d] += _W1[h][d] * gradHidden[h];

            return gradInput;
        }

        public void UpdateWeights(double learningRate)
        {
            int hidden = _W1.Length;
            int dim = _W2.Length;

            for (int h = 0; h < hidden; h++)
            {
                _b1[h] -= learningRate * _gb1[h];
                _gb1[h] = 0;
                for (int d = 0; d < dim; d++)
                {
                    _W1[h][d] -= learningRate * _gW1[h][d];
                    _gW1[h][d] = 0;
                }
            }
            for (int d = 0; d < dim; d++)
            {
                _b2[d] -= learningRate * _gb2[d];
                _gb2[d] = 0;
                for (int h = 0; h < hidden; h++)
                {
                    _W2[d][h] -= learningRate * _gW2[d][h];
                    _gW2[d][h] = 0;
                }
            }
        }

        // Pour sauvegarde/chargement
        public (double[][] W1, double[] b1, double[][] W2, double[] b2) GetWeights()
        {
            return (
                _W1.Select(r => (double[])r.Clone()).ToArray(),
                (double[])_b1.Clone(),
                _W2.Select(r => (double[])r.Clone()).ToArray(),
                (double[])_b2.Clone()
            );
        }

        public void LoadWeights(double[][] W1, double[] b1, double[][] W2, double[] b2)
        {
            _W1 = W1.Select(r => (double[])r.Clone()).ToArray();
            _b1 = (double[])b1.Clone();
            _W2 = W2.Select(r => (double[])r.Clone()).ToArray();
            _b2 = (double[])b2.Clone();

            int hidden = _W1.Length;
            int dim = _W2.Length;
            _gW1 = Enumerable.Range(0, hidden).Select(_ => new double[dim]).ToArray();
            _gb1 = new double[hidden];
            _gW2 = Enumerable.Range(0, dim).Select(_ => new double[hidden]).ToArray();
            _gb2 = new double[dim];
        }
    }
}
