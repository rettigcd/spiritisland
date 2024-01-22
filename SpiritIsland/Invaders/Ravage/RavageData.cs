namespace SpiritIsland;

/// <summary>
/// One instance is created for each Ravage on each space
/// </summary>
public class RavageData( SpaceState tokens ) {

	#region constructor

	#endregion

	public readonly List<RavageExchange> Result = []; // record status here
	public readonly SpaceState Tokens = tokens;

	public InvaderBinding InvaderBinding;

}