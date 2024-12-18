namespace SpiritIsland;

/// <summary>
/// One instance is created for each Ravage on each space
/// </summary>
public class RavageData( Space space ) {

	#region constructor

	#endregion

	public readonly List<RavageExchange> Result = []; // record status here
	public readonly Space Space = space;

	public InvaderBinding InvaderBinding = space.Invaders;

}