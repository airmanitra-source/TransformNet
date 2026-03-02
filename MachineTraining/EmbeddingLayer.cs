namespace MachineLearning.ApiService.Models
{
    //L'Embedding transforme un index (ex: 5) en un vecteur de double (ex: [0.1, -0.5, 0.2]). Ces valeurs sont apprises par le réseau.
    /*1. FORWARD(Prédiction)
   ↓
   "chat" → embedding[0.23, -0.51, 0.12, 0.89]  ← Utilisé pour prédire
   ↓
   Attention, FeedForward... → Prédiction finale
   ↓
   Erreur = Cible - Prédiction = 0.05

2. BACKWARD(Apprentissage)
   ↓
   "Cette erreur est due en partie à mon embedding !"
   ↓
   Calcul du gradient : ∇embedding = "Combien changer le vecteur [0.23, -0.51, ...]"
   ↓
   Mise à jour : embedding += tauxApprentissage × gradient

   Résultat : "chat" → [0.24, -0.50, 0.13, 0.88]  ← Légèrement ajusté
        !*/
    public class EmbeddingLayer
    {
        private double[][] _table; // Matrice [TailleVocabulaire x Dimension]
        private double[][] _gradients; // Gradients pour l'apprentissage
        private Random _rand = new Random();

        public EmbeddingLayer(int tailleVocab, int dimension)
        {
            _table = new double[tailleVocab][];
            _gradients = new double[tailleVocab][];
            for (int i = 0; i < tailleVocab; i++)
            {
                // Initialisation aléatoire des vecteurs de sens
                _table[i] = Enumerable.Range(0, dimension).Select(_ => _rand.NextDouble() * 2 - 1).ToArray();
                _gradients[i] = new double[dimension]; // Zéro par défaut
            }
        }

        public double[] GetCurrentVectorEmbeddingAtIndex(int id)
        {
            if (id == -1)
                return new double[_table[0].Length];
            // Retourner une COPIE pour éviter les mutations accidentelles
            return (double[])_table[id].Clone();
        }

        public void AjouterGradient(int id, double[] gradient)
        {
            if (id == -1) return;
            for (int i = 0; i < gradient.Length; i++)
                _gradients[id][i] += gradient[i];
        }

        public void MettreAJourPoids(double tauxApprentissage)
        {
            for (int i = 0; i < _table.Length; i++)
            {
                for (int j = 0; j < _table[i].Length; j++)
                {
                    _table[i][j] -= tauxApprentissage * _gradients[i][j];
                    _gradients[i][j] = 0; // Réinitialiser pour la prochaine itération
                }
            }
        }

        public double[][] ObtenirTable()
        {
            // Retourner une copie de la table pour la sauvegarde
            return _table.Select(row => (double[])row.Clone()).ToArray();
        }

        public void ChargerTable(double[][] nouvelleTable)
        {
            _table = nouvelleTable.Select(row => (double[])row.Clone()).ToArray();
            // Réinitialiser les gradients après chargement
            _gradients = new double[_table.Length][];
            for (int i = 0; i < _table.Length; i++)
            {
                _gradients[i] = new double[_table[i].Length];
            }
        }
    }

}