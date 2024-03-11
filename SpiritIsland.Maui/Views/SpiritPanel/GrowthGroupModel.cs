namespace SpiritIsland.Maui;

public class GrowthGroupModel( GrowthGroup gg ) : ObservableModel {

	public GrowthActionModel[] Actions { 
		get => _actions; 
		private set => SetProp(ref _actions,value);
	}

	GrowthActionModel[] _actions = gg.GrowthActions
			.Select(a => new GrowthActionModel((SpiritGrowthAction)a))
			.ToArray();
}