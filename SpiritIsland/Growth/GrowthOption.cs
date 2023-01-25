namespace SpiritIsland;

/// <summary>
/// Indivisible group of actions, all of which must be resolved.
/// </summary>
public class GrowthOption : IOption { // This is an IOption so Startlight can select which option to add.

	/// <summary> When negative, prevents growth option unless user has sufficient energy to satisfy </summary>
	public int GainEnergy = 0;

	public GrowthOption(params GrowthActionFactory[] actions){ 
		this.GrowthActions = actions; 
	}

	public GrowthActionFactory[] GrowthActions { get; }

	public IEnumerable<GrowthActionFactory> AutoRuns => GrowthActions.Where(x=>x.AutoRun);

	public IEnumerable<GrowthActionFactory> UserRuns => GrowthActions.Where( x => !x.AutoRun );

	public string Text => ToString();

	public override string ToString() {
		return GrowthActions.Select(a=>a.Name).Join(" / ");
	}

}