namespace SpiritIsland;

/// <summary>
/// Indivisible group of actions, all of which must be resolved.
/// </summary>
public class GrowthGroup( params IActOn<Spirit>[] actions ) 
	: IOption
{ // This is an IOption so Startlight can select which option to add.

	/// <summary> When negative, prevents growth option unless user has sufficient energy to satisfy </summary>
	public int GainEnergy = 0;

	/// <summary>
	/// The things that get put in the spirits unUsedAction/factory list
	/// </summary>
	public IHelpGrowActionFactory[] GrowthActionFactories => _growthActionFactories ??= actions.Select( action => new GrowthAction( action ) ).ToArray();
	IHelpGrowActionFactory[]? _growthActionFactories = null;

	/// <summary>
	/// The simple Description/ActAsync commands
	/// </summary>
	public IActOn<Spirit>[] Actions => actions;

	string IOption.Text => ToString();

	public override string ToString() => GrowthActionFactories.Select(a=>a.Title).Join(" / ");

}