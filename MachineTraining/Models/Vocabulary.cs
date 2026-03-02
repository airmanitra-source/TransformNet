namespace MachineTraining.Models
{
    using System.Collections.Generic;
    
    public class Vocabulary
    {
        // Tokens spéciaux réservés
        public const int PAD = 0;   // Padding (remplissage)
        public const int UNK = 1;   // Unknown (mot inconnu)
        public const int BOS = 2;   // Beginning Of Sequence
        public const int EOS = 3;   // End Of Sequence
        private const int PREMIER_TOKEN = 4; // Premier index après les tokens spéciaux

        private Dictionary<string, int> _motVersId = new Dictionary<string, int>();
        private List<string> _idVersMot = new List<string>();

        public Vocabulary(string data)
        {
            // Initialiser les tokens spéciaux
            _motVersId["<pad>"] = PAD;
            _motVersId["<unk>"] = UNK;
            _motVersId["<bos>"] = BOS;
            _motVersId["<eos>"] = EOS;
            _idVersMot.Add("<pad>");
            _idVersMot.Add("<unk>");
            _idVersMot.Add("<bos>");
            _idVersMot.Add("<eos>");
            // Nettoyer et tokeniser avec suppression des espaces vides
            var mots = data.ToLower()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\t' },
                       StringSplitOptions.RemoveEmptyEntries);

            foreach (var m in mots)
            {
                if (!_motVersId.ContainsKey(m))
                {
                    _motVersId[m] = _idVersMot.Count;
                    _idVersMot.Add(m);
                }
            }
        }

        public int GetId(string mot)
        {
            string motLower = mot.ToLower();
            return _motVersId.ContainsKey(motLower) ? _motVersId[motLower] : UNK;
        }

        public string GetMot(int id)
        {
            return id >= 0 && id < _idVersMot.Count ? _idVersMot[id] : "<unk>";
        }

        public int Taille => _idVersMot.Count;

        // ✅ Propriété pour sauvegarde
        public List<string> ListeMots => new List<string>(_idVersMot);

        // ✅ Méthode pour compter les mots
        public int GetWordsCount() => _idVersMot.Count;

        // ✅ Méthode pour tokeniser un texte
        public double[][] Tokeniser(string texte, EmbeddingLayer embedding)
        {
            var mots = texte.ToLower()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\t' }, 
                       StringSplitOptions.RemoveEmptyEntries);

            List<double[]> tokens = new List<double[]>();
            foreach (var mot in mots)
            {
                int id = GetId(mot);
                tokens.Add(embedding.GetCurrentVectorEmbeddingAtIndex(id));
            }

            return tokens.ToArray();
        }

        // ✅ Méthode pour extraire les cibles (mot suivant)
        public double[] GetTarget(string texte)
        {
            var mots = texte.ToLower()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\t' }, 
                       StringSplitOptions.RemoveEmptyEntries);

            // Les cibles sont les ID du mot suivant pour chaque mot
            List<double> cibles = new List<double>();
            for (int i = 0; i < mots.Length - 1; i++)
            {
                cibles.Add((double)GetId(mots[i + 1]));
            }

            return cibles.ToArray();
        }
    }

}
