namespace SpiritIsland.Tests;

static internal class Extensions {

	public static Task PlaceOn( this SpiritPresence presence, Space space, GameState gameState )
		=> presence.PlaceOn( gameState.Tokens[space], Guid.NewGuid() );

}
