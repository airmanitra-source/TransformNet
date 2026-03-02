namespace MachineTraining.Models.Utils
{
    public class TextUtils
    {
        public int ChoisirIndex(double[] scores, double temperature)
        {
            // 1. Appliquer la température (Éviter T = 0 pour la division)
            double t = Math.Max(temperature, 0.01);
            double[] scoresAjustes = scores.Select(s => s / t).ToArray();

            // 2. Softmax avec stabilité numérique
            double maxScore = scoresAjustes.Max();
            double[] exp = scoresAjustes.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = exp.Sum();
            double[] probas = exp.Select(e => e / somme).ToArray();

            // 3. Échantillonnage aléatoire (Sampling) au lieu de l'Argmax
            // Cela permet au modèle de ne pas toujours dire la même chose
            double seuil = Random.Shared.NextDouble();
            double cumul = 0;

            for (int i = 0; i < probas.Length; i++)
            {
                cumul += probas[i];
                if (seuil <= cumul) return i;
            }

            return probas.Length - 1;
        }

        // ✅ NOUVEAU: Top-K Sampling pour éviter les répétitions
        public int ChoisirIndexTopK(double[] scores, double temperature, int topK = 5)
        {
            // 1. Appliquer la température
            double t = Math.Max(temperature, 0.01);
            double[] scoresAjustes = scores.Select(s => s / t).ToArray();

            // 2. Softmax
            double maxScore = scoresAjustes.Max();
            double[] exp = scoresAjustes.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = exp.Sum();
            double[] probas = exp.Select(e => e / somme).ToArray();

            // 3. Top-K: Garder seulement les K meilleurs indices
            var topKIndices = Enumerable.Range(0, probas.Length)
                .OrderByDescending(i => probas[i])
                .Take(topK)
                .ToList();

            // 4. Normaliser les probabilités du top-K
            double sommeTopK = topKIndices.Sum(i => probas[i]);
            Dictionary<int, double> probasNormalisees = new Dictionary<int, double>();
            foreach (var idx in topKIndices)
            {
                probasNormalisees[idx] = probas[idx] / sommeTopK;
            }

            // 5. Échantillonner parmi le top-K
            double seuil = Random.Shared.NextDouble();
            double cumul = 0;

            foreach (var idx in topKIndices)
            {
                cumul += probasNormalisees[idx];
                if (seuil <= cumul) return idx;
            }

            return topKIndices.Last();
        }

        // ✅ NOUVEAU: Nucleus Sampling (Top-P) - Plus stable que Top-K
        public int ChoisirIndexNucleusP(double[] scores, double temperature, double p = 0.9, string[] motHistorique = null)
        {
            // 1. Appliquer la température
            double t = Math.Max(temperature, 0.01);
            double[] scoresAjustes = scores.Select(s => s / t).ToArray();

            // 2. Softmax
            double maxScore = scoresAjustes.Max();
            double[] exp = scoresAjustes.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = exp.Sum();
            double[] probas = exp.Select(e => e / somme).ToArray();

            // 3. Trier par probabilité décroissante
            var indices = Enumerable.Range(0, probas.Length)
                .OrderByDescending(i => probas[i])
                .ToList();

            // 4. Accumuler jusqu'à atteindre p
            double cumul = 0;
            var topPIndices = new List<int>();
            foreach (var idx in indices)
            {
                cumul += probas[idx];
                topPIndices.Add(idx);
                if (cumul >= p) break;
            }

            // 5. Renormaliser
            double summeTopP = topPIndices.Sum(i => probas[i]);
            Dictionary<int, double> probasNormalisees = new Dictionary<int, double>();
            foreach (var idx in topPIndices)
            {
                probasNormalisees[idx] = probas[idx] / summeTopP;
            }

            // 6. Penaliser les mots trop récents pour éviter les répétitions
            if (motHistorique != null && motHistorique.Length > 0)
            {
                // Réduire la probabilité du dernier mot (éviter "de de de")
                string dernierMot = motHistorique.Last();
                // Cette partie serait dans l'appelant, donc on peut pas le faire ici facilement
            }

            // 7. Échantillonner
            double seuil = Random.Shared.NextDouble();
            double cumulProba = 0;

            foreach (var idx in topPIndices)
            {
                cumulProba += probasNormalisees[idx];
                if (seuil <= cumulProba) return idx;
            }

            return topPIndices.Last();
        }

        public string PredireProchainMot(double[] scores, Vocabulary vocab)
        {
            // 1. Softmax (pour transformer en probabilités 0-1)
            double maxScore = scores.Max();
            double[] expScores = scores.Select(s => Math.Exp(s - maxScore)).ToArray();
            double somme = expScores.Sum();
            double[] probas = expScores.Select(p => p / somme).ToArray();

            // 2. Argmax (trouver l'index du score le plus haut)
            int indexGagnant = 0;
            double scoreMax = -1;
            for (int i = 0; i < probas.Length; i++)
            {
                if (probas[i] > scoreMax)
                {
                    scoreMax = probas[i];
                    indexGagnant = i;
                }
            }

            // 3. Traduction Index -> Mot
            return vocab.GetMot(indexGagnant);
        }
    }

}
