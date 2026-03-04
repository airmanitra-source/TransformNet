namespace MachineLearning.Web.Models;

/// <summary>
/// Contrat qu'implémente chaque ViewModel pour se construire depuis un modèle métier.
/// Évite de dupliquer les calculs entre la couche domaine et la couche présentation.
/// </summary>
/// <typeparam name="TBusiness">Type du modèle métier source</typeparam>
/// <typeparam name="TSelf">Type du ViewModel (self-referencing pour la factory statique)</typeparam>
public interface IFromBusinessModel<TBusiness, TSelf> where TSelf : new()
{
    /// <summary>
    /// Remplit ce ViewModel à partir du modèle métier fourni.
    /// </summary>
    void FromBusinessModel(TBusiness model);

    /// <summary>
    /// Crée et retourne un nouveau ViewModel initialisé depuis le modèle métier.
    /// </summary>
    static TSelf From(TBusiness model)
    {
        var vm = new TSelf();
        ((IFromBusinessModel<TBusiness, TSelf>)vm).FromBusinessModel(model);
        return vm;
    }
}
