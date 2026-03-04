namespace Price.Module
{
    /// <summary>
    /// Contrat de gestion des prix et ajustements d'élasticité.
    /// Encapsule les mécanismes d'inflation liée au carburant et d'aléa de marché.
    /// </summary>
    public interface IPriceModule
    {
        /// <summary>
        /// Calcule le prix ajusté d'une marchandise locale selon la variation du prix du carburant.
        /// Applique une élasticité paramétrable : prix_final = prix_base * (1 + élasticité * Δcarburant/carburant_ref)
        /// Plus aléa ~ N(0, σ_aléa)
        /// </summary>
        double AjusterPrixParCarburant(
            double prixBase,
            double prixCarburantCourant,
            double prixCarburantReference,
            double elasticitePrix,
            double volatiliteAlea,
            Random random
        );

        /// <summary>
        /// Calcule le coût d'achat de produits alimentaires après ajustement prix.
        /// Tient compte du partage informel/formel et des effets de revenu.
        /// </summary>
        double CalculerCoutPanierAlimentaire(
            double depenseAlimentairesJourBase,
            double prixCarburantCourant,
            double prixCarburantReference,
            double elasticitePrix,
            double volatiliteAlea,
            double partRevenuAlimentaire,
            Random random
        );

        /// <summary>
        /// Estime la réduction de quantités achetées suite à des augmentations répétitives de prix.
        /// Utilise une courbe d'ajustement comportemental du ménage.
        /// </summary>
        double CalculerFacteurReductionQuantites(
            double cumulHaussePrix,        // Somme des % d'augmentation successives
            double elasticiteRutilisateur  // Sensibilité du ménage (0.0-1.0)
        );
    }
}
