using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace MachineLearning.ApiService.Models
{
    public class GestionnairePoids
    {
        public void Charger(string chemin, MultiHeadTransformer block, EmbeddingLayer emb)
        {
            if (!File.Exists(chemin))
                throw new FileNotFoundException($"Fichier de poids non trouvé : {chemin}");

            string json = File.ReadAllText(chemin);
            var data = JsonSerializer.Deserialize<ParametresModel>(json);

            if (data == null)
                throw new InvalidOperationException("Impossible de désérialiser les paramètres du modèle");

            // 1. Restaurer l'Embedding
            emb.ChargerTable(data.TableEmbedding);

            // 2. Restaurer le FFN
            if (data.FFN_W1 != null)
                block.FeedForward.LoadWeights(data.FFN_W1, data.FFN_b1, data.FFN_W2, data.FFN_b2);
        }


        public void Sauvegarder(string chemin, MultiHeadTransformer block, EmbeddingLayer emb, Vocabulaire vocab)
        {
            var (w1, b1, w2, b2) = block.FeedForward.GetWeights();

            var paramsModel = new ParametresModel
            {
                Vocabulaire = vocab.ListeMots,
                TableEmbedding = emb.ObtenirTable(),
                FFN_W1 = w1,
                FFN_b1 = b1,
                FFN_W2 = w2,
                FFN_b2 = b2
            };

            string json = JsonSerializer.Serialize(paramsModel, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(chemin, json);
        }
    }

}
