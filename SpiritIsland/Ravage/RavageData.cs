namespace SpiritIsland;

/// <summary>
/// One instance is created for each Ravage on each space
/// </summary>
public class RavageData {

	#region constructor

	public RavageData( SpaceState tokens, GameState gameState ) {
		Tokens = tokens;
		GameState = gameState;
		Result = new InvadersRavaged { Space = tokens.Space };
	}

	#endregion
	public int BadLandsCount => Tokens.Badlands.Count;

	public readonly InvadersRavaged Result; // record status here
	public readonly GameState GameState;
	public readonly SpaceState Tokens;

	public CountDictionary<HealthToken> CurrentAttackers; // tokens might change if strife is removed
	public UnitOfWork ActionScope { get; set; } // This ONLY has a value during the ravage.  Not before nor after.
	public InvaderBinding InvaderBinding;

}