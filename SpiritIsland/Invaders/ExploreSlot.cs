namespace SpiritIsland;

public class ExploreSlot : InvaderSlot {
	public ExploreSlot() : base( "Explore" ) { }

	/// <summary>
	/// Sinlge Entry point for Exploring a card.
	/// </summary>
	public async override Task ActivateCard( InvaderCard card, GameState gameState ) {
		card.Flip();
		await Engine.ActivateCard( card, gameState );
	}

	// The Engine is Configured and replaced for different adversaries.
	public ExploreEngine Engine = new ExploreEngine();
}
