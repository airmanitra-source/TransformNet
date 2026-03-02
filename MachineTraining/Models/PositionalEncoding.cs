namespace MachineTraining.Models
{
    public class PositionalEncoding
    {
        private int _dimension;
        private double[][] _sequence;

        public PositionalEncoding(int dimension)
        {
            _dimension = dimension;
        }

        public double[][] GetPositionalEncoding(double[][] sequence)
        {
            for (int pos = 0; pos < sequence.Length; pos++)
            {
                for (int i = 0; i < _dimension; i++)
                {
                    // Formule standard : PE(pos, 2i) = sin(pos / 10000^(2i/d))
                    //                    PE(pos, 2i+1) = cos(pos / 10000^(2i/d))
                    double angle = pos / Math.Pow(10000, (2.0 * i) / _dimension);

                    if (i % 2 == 0)
                        sequence[pos][i] += Math.Sin(angle);
                    else
                        sequence[pos][i] += Math.Cos(angle);
                }
            }
            _sequence = sequence;
            return sequence;
        }

        // Affichage intelligent du PE initial
        public void Print(int sequenceLength, string label = "Positional Encoding Initial")
        {            
            Console.WriteLine("\n" + new string('=', 100));
            Console.WriteLine($" {label}");
            Console.WriteLine($"   Dimension: {_dimension} | Longueur séquence: {sequenceLength}");
            Console.WriteLine(new string('=', 100));

            // Afficher les premières et dernières positions
            int affichagePositions = Math.Min(5, sequenceLength);
            
            for (int pos = 0; pos < affichagePositions; pos++)
            {
                Console.WriteLine($"\n   Position {pos}:");
                Console.Write("   PE[");
                for (int i = 0; i < _dimension; i++)
                {
                    Console.Write($"{_sequence[pos][i],8:F4}");
                    if (i < _dimension - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Si la séquence est longue, afficher aussi la dernière position
            if (sequenceLength > affichagePositions)
            {
                int lastPos = sequenceLength - 1;
                Console.WriteLine($"\n   Position {lastPos} (dernière):");
                Console.Write("   PE[");
                for (int i = 0; i < _dimension; i++)
                {
                    Console.Write($"{_sequence[lastPos][i],8:F4}");
                    if (i < _dimension - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Statistiques
            Console.WriteLine("\n   Statistiques:");
            double minVal = _sequence.SelectMany(x => x).Min();
            double maxVal = _sequence.SelectMany(x => x).Max();
            double moyVal = _sequence.SelectMany(x => x).Average();
            double stdDev = Math.Sqrt(_sequence.SelectMany(x => x)
                .Average(x => Math.Pow(x - moyVal, 2)));
            
            Console.WriteLine($"   Min: {minVal:F6} | Max: {maxVal:F6}");
            Console.WriteLine($"   Moyenne: {moyVal:F6} | Écart-type: {stdDev:F6}");
            Console.WriteLine(new string('=', 80) + "\n");
        }
    }

}
