namespace Price.Module;

/// <summary>
/// Résultat du calcul de la règle de Taylor pour le taux directeur BCM.
///
/// Décompose le taux directeur en ses composantes économiques :
///   - Taux réel neutre (r*) : rendement réel d'équilibre
///   - Inflation courante (π) : niveau actuel des prix
///   - Écart d'inflation (π - π*) : surréaction si inflation > cible
///   - Output gap : surchauffe (+) ou sous-utilisation (-)
///
/// Le taux final est lissé pour refléter l'inertie institutionnelle de la BCM
/// (historiquement, le taux directeur BCM bouge de ±50 bps/trimestre maximum).
/// </summary>
public class TaylorRuleResult
{
    /// <summary>
    /// Taux directeur calculé par la règle de Taylor (annuel, ex: 0.095 = 9.5%).
    /// C'est le taux "idéal" selon les fondamentaux.
    /// </summary>
    public double TauxDirecteurTaylor { get; set; }

    /// <summary>
    /// Taux directeur effectif après lissage (annuel).
    /// C'est cette valeur qui remplace le taux directeur dans Government.
    /// Le lissage reflète l'inertie décisionnelle de la BCM.
    /// </summary>
    public double TauxDirecteurEffectif { get; set; }

    /// <summary>
    /// Composante taux réel neutre (r*). Ex: 0.02 = 2%.
    /// </summary>
    public double ComposanteTauxReelNeutre { get; set; }

    /// <summary>
    /// Composante inflation courante (π). Ex: 0.08 = 8%.
    /// </summary>
    public double ComposanteInflation { get; set; }

    /// <summary>
    /// Composante écart d'inflation : α × (π - π*). Positif si inflation au-dessus de la cible.
    /// </summary>
    public double ComposanteEcartInflation { get; set; }

    /// <summary>
    /// Composante output gap : β × output_gap. Positif si surchauffe.
    /// </summary>
    public double ComposanteOutputGap { get; set; }

    /// <summary>
    /// Écart entre le taux Taylor et le taux précédent (signal de la direction).
    /// Positif = resserrement monétaire, négatif = assouplissement.
    /// </summary>
    public double EcartTaylorVsPrecedent { get; set; }

    /// <summary>
    /// Variation effective appliquée au taux directeur (après lissage).
    /// </summary>
    public double VariationEffective { get; set; }
}
