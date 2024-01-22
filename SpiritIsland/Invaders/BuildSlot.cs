namespace SpiritIsland;

public class BuildSlot( string label = "Build" ) : InvaderSlot( label ) {

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await Engine.ActivateCard( card, gameState );
	}

	public BuildEngine Engine { get; set; } = new BuildEngine();

}
