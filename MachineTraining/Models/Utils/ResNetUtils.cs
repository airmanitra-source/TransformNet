namespace MachineTraining.Models.Utils
{
    public class ResNetUtils
    {
        // Additionne l'entrée originale au résultat de la couche
        public static double[] Ajouter(double[] entree, double[] sortieCouche)
        {
            if (entree.Length != sortieCouche.Length)
                throw new ArgumentException("Les dimensions doivent correspondre !");

            double[] resultat = new double[entree.Length];
            for (int i = 0; i < entree.Length; i++)
                resultat[i] = entree[i] + sortieCouche[i];

            return resultat;
        }
    }
}
