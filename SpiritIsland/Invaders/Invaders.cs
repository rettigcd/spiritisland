namespace SpiritIsland;

// !!! rethink this class.  Are things calling gameState.Invaders.On(space) when they should really be calling gameState.Tokens[space].Invaders ???
public class Invaders {

	// !! This wrapper class (around TokenCountDictionary) acts more like an Extension Method Class

	readonly GameState gs;

	#region constructor

	public Invaders( GameState gs ) {
		this.gs = gs;
	}

	#endregion

	public InvaderBinding On( Space targetSpace, UnitOfWork actionScope ) {
		return new InvaderBinding( gs.Tokens[targetSpace], actionScope );
	}

}
