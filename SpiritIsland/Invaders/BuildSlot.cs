namespace SpiritIsland;

public class BuildSlot : InvaderSlot {

	public BuildSlot(string label="Build") : base( label ) { }

	public override async Task ActivateCard( IInvaderCard card, GameState gameState ) {
		await Engine.ActivateCard( card, gameState );
	}

	public BuildEngine Engine { get; set; } = new BuildEngine();

	static public Task DefaultBuild(GameState gameState, SpaceState spaceState) => new BuildOnceOnSpace( gameState, spaceState ).Exec();
}
