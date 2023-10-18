namespace SpiritIsland;

/// <summary>
/// One instance is created for each Ravage on each space
/// </summary>
public class RavageData {

	#region constructor

	public RavageData( SpaceState tokens ) {
		Tokens = tokens;
		Result = new List<RavageExchange>();
	}

	#endregion

	public readonly List<RavageExchange> Result; // record status here
	public readonly SpaceState Tokens;

	public InvaderBinding InvaderBinding;

}