using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Implémentation du module input-output simplifié.
/// Matrice 7×7 calibrée sur les données INSTAT/SCN Madagascar (TRE 2019).
///
/// La matrice modélise les coefficients techniques aᵢⱼ : quand le secteur j
/// dépense 1 MGA en intrants, quelle part va au secteur i ?
///
/// Exemple : quand la Construction achète des intrants :
///   - 25% va à l'Agriculture (bois, paille, sisal)
///   - 10% aux Textiles (sacs, bâches)
///   - 20% aux Commerces (quincaillerie, ciment importé)
///   - 15% aux Services (transport, logistique)
///   - 20% au SecteurMinier (sable, pierre, fer)
///   - 5% à la Construction (sous-traitance BTP)
///   - 5% à l'HôtellerieTourisme (restauration ouvriers)
/// </summary>
public class InputOutputModule : IInputOutputModule
{
    /// <summary>
    /// Matrice des coefficients techniques aᵢⱼ.
    /// Clé : (fournisseur_i, acheteur_j), Valeur : coefficient (0-1).
    /// Chaque colonne (acheteur) doit sommer à ~1.0 ou moins.
    ///
    /// Calibrage : INSTAT TRE 2019, SCN Madagascar, données GTAP.
    /// Les coefficients reflètent la structure économique malgache :
    ///   - Agriculture : peu de transformation, achats limités
    ///   - Construction : fort effet d'entraînement multi-sectoriel
    ///   - Minier : capitalistique, achète beaucoup de services et d'énergie
    /// </summary>
    private static readonly Dictionary<(ESecteurActivite Fournisseur, ESecteurActivite Acheteur), double> CoefficientsTechniques = new()
    {
        // ═══════════════════════════════════════════════════════════════
        //  Colonne AGRICULTURE (acheteur) — faible CI, surtout auto-approvisionnement
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.Agriculture), 0.15 },    // semences, engrais organiques
        { (ESecteurActivite.Commerces, ESecteurActivite.Agriculture), 0.10 },      // intrants commerciaux (engrais chimiques, outils)
        { (ESecteurActivite.Services, ESecteurActivite.Agriculture), 0.05 },       // transport récolte
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.Agriculture), 0.02 },  // phosphate (engrais)
        { (ESecteurActivite.Construction, ESecteurActivite.Agriculture), 0.03 },   // silos, irrigation
        { (ESecteurActivite.Textiles, ESecteurActivite.Agriculture), 0.02 },       // sacs jute
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.Agriculture), 0.00 },

        // ═══════════════════════════════════════════════════════════════
        //  Colonne TEXTILES (acheteur) — chaîne d'approvisionnement ZFI
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.Textiles), 0.20 },       // coton, sisal, soie
        { (ESecteurActivite.Textiles, ESecteurActivite.Textiles), 0.10 },          // fils, tissus intermédiaires
        { (ESecteurActivite.Commerces, ESecteurActivite.Textiles), 0.15 },         // import tissu, teinture
        { (ESecteurActivite.Services, ESecteurActivite.Textiles), 0.10 },          // transport, logistique
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.Textiles), 0.00 },
        { (ESecteurActivite.Construction, ESecteurActivite.Textiles), 0.02 },      // entretien locaux
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.Textiles), 0.01 },

        // ═══════════════════════════════════════════════════════════════
        //  Colonne COMMERCES (acheteur) — achats pour revente
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.Commerces), 0.15 },      // produits agricoles à revendre
        { (ESecteurActivite.Textiles, ESecteurActivite.Commerces), 0.05 },         // vêtements, sacs
        { (ESecteurActivite.Commerces, ESecteurActivite.Commerces), 0.10 },        // grossiste → détaillant
        { (ESecteurActivite.Services, ESecteurActivite.Commerces), 0.12 },         // transport, assurance
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.Commerces), 0.02 },    // matériaux (quincaillerie)
        { (ESecteurActivite.Construction, ESecteurActivite.Commerces), 0.03 },     // locaux commerciaux
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.Commerces), 0.01 },

        // ═══════════════════════════════════════════════════════════════
        //  Colonne SERVICES (acheteur) — services aux entreprises
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.Services), 0.05 },       // restauration, alimentation
        { (ESecteurActivite.Textiles, ESecteurActivite.Services), 0.02 },          // uniformes, fournitures
        { (ESecteurActivite.Commerces, ESecteurActivite.Services), 0.15 },         // fournitures bureau, équipement
        { (ESecteurActivite.Services, ESecteurActivite.Services), 0.15 },          // sous-traitance services
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.Services), 0.01 },
        { (ESecteurActivite.Construction, ESecteurActivite.Services), 0.05 },      // locaux, aménagement
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.Services), 0.02 },

        // ═══════════════════════════════════════════════════════════════
        //  Colonne SECTEUR MINIER (acheteur) — capitalistique, énergie
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.SecteurMinier), 0.02 },
        { (ESecteurActivite.Textiles, ESecteurActivite.SecteurMinier), 0.01 },
        { (ESecteurActivite.Commerces, ESecteurActivite.SecteurMinier), 0.15 },    // import équipements
        { (ESecteurActivite.Services, ESecteurActivite.SecteurMinier), 0.20 },     // services techniques, logistique
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.SecteurMinier), 0.10 }, // sous-traitance minière
        { (ESecteurActivite.Construction, ESecteurActivite.SecteurMinier), 0.08 }, // BTP mines
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.SecteurMinier), 0.01 },

        // ═══════════════════════════════════════════════════════════════
        //  Colonne CONSTRUCTION (acheteur) — fort effet d'entraînement
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.Construction), 0.08 },   // bois, paille, sisal
        { (ESecteurActivite.Textiles, ESecteurActivite.Construction), 0.03 },      // bâches, sacs
        { (ESecteurActivite.Commerces, ESecteurActivite.Construction), 0.22 },     // quincaillerie, ciment importé
        { (ESecteurActivite.Services, ESecteurActivite.Construction), 0.15 },      // transport matériaux, logistique
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.Construction), 0.20 }, // sable, pierre, fer
        { (ESecteurActivite.Construction, ESecteurActivite.Construction), 0.08 },  // sous-traitance BTP
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.Construction), 0.02 }, // restauration chantier

        // ═══════════════════════════════════════════════════════════════
        //  Colonne HÔTELLERIE/TOURISME (acheteur)
        // ═══════════════════════════════════════════════════════════════
        { (ESecteurActivite.Agriculture, ESecteurActivite.HotellerieTourisme), 0.25 },  // alimentation, produits frais
        { (ESecteurActivite.Textiles, ESecteurActivite.HotellerieTourisme), 0.05 },     // linge, textiles
        { (ESecteurActivite.Commerces, ESecteurActivite.HotellerieTourisme), 0.15 },    // approvisionnement divers
        { (ESecteurActivite.Services, ESecteurActivite.HotellerieTourisme), 0.15 },     // transport touristes, guides
        { (ESecteurActivite.SecteurMinier, ESecteurActivite.HotellerieTourisme), 0.00 },
        { (ESecteurActivite.Construction, ESecteurActivite.HotellerieTourisme), 0.05 }, // entretien bâtiments
        { (ESecteurActivite.HotellerieTourisme, ESecteurActivite.HotellerieTourisme), 0.05 }, // sous-traitance hôtelière
    };

    /// <summary>
    /// Multiplicateurs de production de Leontief simplifiés par secteur.
    /// Pré-calculés à partir de la matrice inverse de Leontief (I - A)⁻¹.
    /// Indiquent l'effet total (direct + indirect) d'1 MGA de demande finale.
    ///
    /// Calibrage : INSTAT TRE 2019, données GTAP Madagascar.
    /// </summary>
    private static readonly Dictionary<ESecteurActivite, double> MultiplicateursProduction = new()
    {
        { ESecteurActivite.Agriculture, 1.35 },        // Faible transformation
        { ESecteurActivite.Textiles, 1.70 },           // Chaîne d'approvisionnement longue
        { ESecteurActivite.Commerces, 1.55 },          // Intermédiaire
        { ESecteurActivite.Services, 1.50 },           // Services divers
        { ESecteurActivite.SecteurMinier, 1.60 },      // Capitalistique
        { ESecteurActivite.Construction, 2.00 },       // Fort effet d'entraînement
        { ESecteurActivite.HotellerieTourisme, 1.75 }, // Tourisme tire agriculture + services
    };

    /// <summary>
    /// Multiplicateurs d'emploi par secteur.
    /// Nombre d'emplois indirects créés pour 1 emploi direct dans le secteur.
    /// </summary>
    private static readonly Dictionary<ESecteurActivite, double> MultiplicateursEmploi = new()
    {
        { ESecteurActivite.Agriculture, 1.15 },        // Peu de sous-traitance
        { ESecteurActivite.Textiles, 1.40 },           // Chaîne textile emploie en amont
        { ESecteurActivite.Commerces, 1.30 },
        { ESecteurActivite.Services, 1.25 },
        { ESecteurActivite.SecteurMinier, 1.50 },      // Beaucoup de sous-traitance
        { ESecteurActivite.Construction, 1.80 },       // BTP = fort créateur d'emploi indirect
        { ESecteurActivite.HotellerieTourisme, 1.55 }, // Tourisme tire l'emploi local
    };

    /// <inheritdoc/>
    public double GetCoefficientTechnique(ESecteurActivite fournisseur, ESecteurActivite acheteur)
    {
        return CoefficientsTechniques.GetValueOrDefault((fournisseur, acheteur), 0.0);
    }

    /// <inheritdoc/>
    public InputOutputResult CalculerFluxInterSectoriels(
        Dictionary<ESecteurActivite, double> achatsB2BParSecteur)
    {
        var result = new InputOutputResult();
        var secteurs = Enum.GetValues<ESecteurActivite>();

        // Initialiser la demande reçue par secteur
        foreach (var s in secteurs)
            result.DemandeRecueParSecteur[s] = 0;

        // Calculer les flux inter-sectoriels
        foreach (var acheteur in secteurs)
        {
            double achatsTotal = achatsB2BParSecteur.GetValueOrDefault(acheteur, 0);
            if (achatsTotal <= 0) continue;

            foreach (var fournisseur in secteurs)
            {
                double coeff = GetCoefficientTechnique(fournisseur, acheteur);
                double flux = achatsTotal * coeff;

                if (flux > 0)
                {
                    result.FluxInterSectoriels[(acheteur, fournisseur)] = flux;
                    result.DemandeRecueParSecteur[fournisseur] += flux;
                    result.TotalConsommationsIntermediaires += flux;
                }
            }
        }

        // Ajouter les multiplicateurs
        foreach (var s in secteurs)
            result.MultiplicateursProduction[s] = GetMultiplicateurProduction(s);

        return result;
    }

    /// <inheritdoc/>
    public double GetMultiplicateurProduction(ESecteurActivite secteur)
    {
        return MultiplicateursProduction.GetValueOrDefault(secteur, 1.0);
    }

    /// <inheritdoc/>
    public double GetMultiplicateurEmploi(ESecteurActivite secteur)
    {
        return MultiplicateursEmploi.GetValueOrDefault(secteur, 1.0);
    }

    /// <inheritdoc/>
    public double CalculerDemandeInduite(
        ESecteurActivite secteurFournisseur,
        Dictionary<ESecteurActivite, double> achatsB2BParSecteur)
    {
        double demandeInduite = 0;
        foreach (var acheteur in Enum.GetValues<ESecteurActivite>())
        {
            double achats = achatsB2BParSecteur.GetValueOrDefault(acheteur, 0);
            double coeff = GetCoefficientTechnique(secteurFournisseur, acheteur);
            demandeInduite += achats * coeff;
        }
        return demandeInduite;
    }
}
