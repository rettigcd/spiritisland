namespace SpiritIsland.Maui;

/// <summary>
/// The view model for a GrowthGroup
/// </summary>
public class GrowthGroupModel( GrowthGroup gg ) : ObservableModel {

	public GrowthActionModel[] Actions => _actions;

	readonly GrowthActionModel[] _actions = [.. gg.GrowthActionFactories.Select(a => new GrowthActionModel((GrowthAction)a))];

}