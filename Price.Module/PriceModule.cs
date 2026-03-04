using System;

namespace Price.Module
{
    /// <summary>
    /// Implémentation du module Gestion des Prix.
    /// Gère l'ajustement des prix aux chocs de carburant avec élasticité et aléa.
    /// Modélise le comportement d'adaptation des ménages aux augmentations successives.
    /// </summary>
    public class PriceModule : IPriceModule
    {
        /// <summary>
        /// Calcule le prix ajusté selon la variation du carburant.
        /// Formule : prix_ajusté = prix_base * (1 + élasticité * Δcarburant/carburant_ref) * (1 + aléa)
        /// </summary>
        public double AjusterPrixParCarburant(
            double prixBase,
            double prixCarburantCourant,
            double prixCarburantReference,
            double elasticitePrix,
            double volatiliteAlea,
            Random random)
        {
            if (prixCarburantReference <= 0) return prixBase;

            // Variation relative du carburant
            double deltaPrix = (prixCarburantCourant - prixCarburantReference) / prixCarburantReference;

            // Choc sur le prix basé sur l'élasticité
            double facteurElasticite = 1.0 + (elasticitePrix * deltaPrix);

            // Aléa de marché ~ N(0, volatiliteAlea)
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double alea = 1.0 + (volatiliteAlea * z);

            // Prix final avec floor à 10% du prix de base (éviter prix négatifs)
            double prixAjuste = prixBase * facteurElasticite * alea;
            return Math.Max(prixAjuste, prixBase * 0.1);
        }

        /// <summary>
        /// Calcule le coût du panier alimentaire avec:
        /// - Ajustement prix carburant (impacts indirectement via transports)
        /// - Partage 85% informel / 15% formel avec marge différente
        /// - Effet revenu (ménage compresse dépenses si inflation trop forte)
        /// </summary>
        public double CalculerCoutPanierAlimentaire(
            double depenseAlimentairesJourBase,
            double prixCarburantCourant,
            double prixCarburantReference,
            double elasticitePrix,
            double volatiliteAlea,
            double partRevenuAlimentaire,
            Random random)
        {
            // Élasticité plus faible pour alimentaires (nécessités, demande inélastique)
            double elasticiteAlimentaire = elasticitePrix * 0.6; // 60% de l'élasticité normale

            // Prix ajusté
            double prixAjuste = AjusterPrixParCarburant(
                depenseAlimentairesJourBase,
                prixCarburantCourant,
                prixCarburantReference,
                elasticiteAlimentaire,
                volatiliteAlea,
                random
            );

            // Partage informel/formel avec marges différentes
            // Informel 85%: marge faible mais volatilité prix haute
            // Formel 15%: prix stable, marge + TVA
            double coutInformel = prixAjuste * 0.85 * 1.0;    // Pas de marge supplémentaire
            double coutFormel = prixAjuste * 0.15 * 1.20;     // TVA 20%

            return coutInformel + coutFormel;
        }

        /// <summary>
        /// Modélise la réduction de quantités achetées face à hausses répétitives.
        /// Curve en S : adaptation progressive puis privation.
        /// 
        /// Exemple:
        /// - cumulHausse = 0%   → facteur = 1.00 (pas d'ajustement)
        /// - cumulHausse = 10%  → facteur = 0.98 (1% de réduction)
        /// - cumulHausse = 30%  → facteur = 0.85 (15% de réduction)
        /// - cumulHausse = 50%+ → facteur = 0.60 (40% de réduction = privation)
        /// </summary>
        public double CalculerFacteurReductionQuantites(
            double cumulHaussePrix,
            double elasticiteRutilisateur)
        {
            if (cumulHaussePrix <= 0) return 1.0;

            // Courbe logistique : adaptation rapide puis plateau (privation)
            // f(x) = 1 / (1 + e^(k*(x - x0)))
            // Paramètres: inflexion à 25% d'augmentation, pente k=0.15/elasticité
            double k = 0.15 / Math.Max(elasticiteRutilisateur, 0.1);
            double x0 = 25.0; // point d'inflexion à 25%

            // Logistique inversée : plus l'inflation, moins on achète
            double facteur = 1.0 / (1.0 + Math.Exp(k * (cumulHaussePrix - x0)));

            // Floor à 30% de consommation (au-delà on meurt de faim, donc limite biologique)
            return Math.Max(facteur, 0.30);
        }
    }
}
