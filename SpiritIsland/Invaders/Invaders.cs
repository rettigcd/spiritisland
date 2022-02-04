namespace SpiritIsland;

public class Invaders {

	// !! This wrapper class (around TokenCountDictionary) acts more like an Extension Method Class

	readonly GameState gs;

	#region constructor

	public Invaders( GameState gs ) {
		this.gs = gs;
	}

	#endregion

	public InvaderBinding On( Space targetSpace ) {
		return new InvaderBinding( 
			gs.Tokens[targetSpace], 
			new DestroyInvaderStrategy( gs, gs.Fear.AddDirect )
		);
	}

}