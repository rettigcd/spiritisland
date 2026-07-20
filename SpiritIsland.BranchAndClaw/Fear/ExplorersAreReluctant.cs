namespace SpiritIsland.BranchAndClaw;

public class ExplorersAreReluctant : FearCardBase, IFearCard {

	public const string Name = "Explorers are Reluctant";
	public string Text => Name;

	[FearLevel( 1, "During the next normal Explore, skip the lowest-numbered land matching the Invader card on each board." )]
	public override Task Level1( GameState ctx )
		=> Cmd.Adjust1Token("Skip the lowest-numbered land matching the Invader card", new SkipLowestNumberedExplore() )
			.In().EachActiveLand()
			.ActAsync( ctx );

	[FearLevel( 2, "Skip the next normal Explore. During the next Invader phase, draw an adidtional Explore card." )]
	public override Task Level2( GameState ctx ) {
		var deck = ctx.InvaderDeck;
		deck.Explore.SkipNextNormal();
		deck.Explore.HoldNextBack();
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal Explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
	public override Task Level3( GameState ctx ) {
		ctx.AddToAllActiveSpaces(new SkipExploreTo());
		return Task.CompletedTask;
	}

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> FearCardRegistry.Register( nameof( ExplorersAreReluctant ), () => new ExplorersAreReluctant() );

}

sealed public class SkipLowestNumberedExplore : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo, ISerializableSpaceEntity {

	public SkipLowestNumberedExplore() : base() {}

	public Task<bool> Skip( Space space ) {
		// Remove
		space.Adjust( this, -1 );
		// Find Lowest space
		_lowest ??= InitLowest();
		// Return if this is the lowest
		bool isLowestOnABoard = space.SpaceSpec.Boards
			.Any( board => _lowest[board] == space.SpaceSpec);
		return Task.FromResult( isLowestOnABoard );
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;


	static Dictionary<Board, SpaceSpec> InitLowest() {
		GameState gameState = GameState.Current;
		var card = gameState.InvaderDeck.Explore.Cards.FirstOrDefault();
		return card == null ? []
			: gameState.Island.Boards
				.ToDictionary( brd => brd, brd => brd.Spaces.ScopeTokens().First( card.MatchesCard ).SpaceSpec ); // ! assumes every board has at least 1 matching space
	}

	Dictionary<Board, SpaceSpec>? _lowest = null;

	// _lowest isn't captured - it's a deterministic cache re-derived from the current InvaderDeck
	// state the first time Skip() runs, same reasoning as the earlier Space->SpaceSpec fix for
	// this type: nothing is lost by rebuilding it fresh.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	const string Tag = "SkipLowestNumberedExplore";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new SkipLowestNumberedExplore() );
}