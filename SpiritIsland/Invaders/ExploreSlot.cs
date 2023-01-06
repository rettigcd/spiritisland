namespace SpiritIsland;

public class ExploreSlot : InvaderSlot {
	public ExploreSlot() : base( "Explore" ) { }
	public async override Task ActivateCard( InvaderCard card, GameState gameState ) {
		await card.Flip( gameState );
		await Engine.ActivateCard( card, gameState );
	}

	public ExploreEngine Engine = new ExploreEngine();
}
