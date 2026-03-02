namespace MachineLearning.ApiService.Models
{
    public static class VisuEntrainement
    {
        public static void AfficherBarre(int actuel, int total, double erreur)
        {
            int largeur = 30;
            int progression = (int)((double)actuel / total * largeur);
            string barre = new string('█', progression) + new string('░', largeur - progression);

            // \r permet de revenir au début de la ligne sans créer de nouvelle ligne
            Console.Write($"\rÉpoque {actuel}/{total} [{barre}] Erreur: {erreur:F6}  ");
        }

        public static void DessinerGraphique(List<double> historiqueErreurs)
        {
            Console.WriteLine("\n\n--- Graphique de la Perte (Loss) ---");
            double max = historiqueErreurs.Max();
            int hauteurGraph = 10;

            for (int h = hauteurGraph; h >= 0; h--)
            {
                double seuil = max * h / hauteurGraph;
                Console.Write($"{seuil:F4} | ");
                foreach (var err in historiqueErreurs.TakeLast(50))
                { // Affiche les 50 derniers points
                    Console.Write(err >= seuil ? "■" : " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("       " + new string('-', 50) + " (Temps)");
        }
    }

}
