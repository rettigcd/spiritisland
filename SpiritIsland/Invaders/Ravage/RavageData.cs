namespace SpiritIsland;

/// <summary>
/// One instance is created for each Ravage on each space
/// </summary>
public class RavageData {

	#region constructor

	public RavageData( SpaceState tokens ) {
		Tokens = tokens;
		GameState = GameState.Current;
		Result = new InvadersRavaged { Space = tokens.Space };
	}

	#endregion
	public int BadLandsCount => Tokens.Badlands.Count;

	public readonly InvadersRavaged Result; // record status here
	public readonly GameState GameState;
	public readonly SpaceState Tokens;

	public CountDictionary<HumanToken> CurrentAttackers; // tokens might change if strife is removed
	public InvaderBinding InvaderBinding;

}