namespace MachineLearning.ApiService.Models
{
    using System.Text.Json;
    using System.IO;

    public class ParametresModel
    {
        public double[][] TableEmbedding { get; set; }
        public List<string> Vocabulaire { get; set; }

        // Poids du FFN (CoucheFeedForward)
        public double[][] FFN_W1 { get; set; }
        public double[] FFN_b1 { get; set; }
        public double[][] FFN_W2 { get; set; }
        public double[] FFN_b2 { get; set; }
    }

}
