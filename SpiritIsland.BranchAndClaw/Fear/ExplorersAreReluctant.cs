namespace SpiritIsland.BranchAndClaw;

public class ExplorersAreReluctant : FearCardBase, IFearCard {

	public const string Name = "Explorers are Reluctant";
	public string Text => Name;

	[FearLevel( 1, "During the next normal Explore, skip the lowest-numbered land matching the Invader card on each board." )]
	public Task Level1( GameState ctx )
		=> Cmd.Adjust1Token("Skip the lowest-numbered land matching the Invader card", new SkipLowestNumberedExplore() )
			.In().EachActiveLand()
			.ActAsync( ctx );

	[FearLevel( 2, "Skip the next normal Explore. During the next Invader phase, draw an adidtional Explore card." )]
	public Task Level2( GameState ctx ) {
		var deck = ctx.InvaderDeck;
		deck.Explore.SkipNextNormal();
		deck.Explore.HoldNextBack();
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal Explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
	public Task Level3( GameState ctx ) {
		ctx.AddToAllActiveSpaces(new SkipExploreTo());
		return Task.CompletedTask;
	}

}

sealed public class SkipLowestNumberedExplore : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo {

	public SkipLowestNumberedExplore() : base() {}

	public Task<bool> Skip( SpaceState spaceState ) {
		// Remove
		spaceState.Adjust( this, -1 );
		// Find Lowest space
		if(_lowest == null) InitLowest();
		// Return if this is the lowest
		bool isLowestOnABoard = spaceState.Space.Boards
			.Any( board => _lowest[board] == spaceState);
		return Task.FromResult( isLowestOnABoard );
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;


	void InitLowest() {
		GameState gameState = GameState.Current;
		var card = gameState.InvaderDeck.Explore.Cards.FirstOrDefault();
		_lowest = card == null ? []
			: gameState.Island.Boards
				.ToDictionary( brd => brd, brd => brd.Spaces.ScopeTokens().FirstOrDefault( card.MatchesCard ) );
	}

	Dictionary<Board, SpaceState> _lowest = null;
}