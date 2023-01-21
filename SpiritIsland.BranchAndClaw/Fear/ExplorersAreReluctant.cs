namespace SpiritIsland.BranchAndClaw;

public class ExplorersAreReluctant : FearCardBase, IFearCard {

	public const string Name = "Explorers are Reluctant";
	public string Text => Name;

	[FearLevel( 1, "During the next normal explore, skip the lowest-numbered land matching the invader card on each board." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.Adjust1Token("Skip the lowest-numbered land matching the invader card", new SkipLowestNumberedExplore(Name) )
			.In().EachActiveLand()
			.Execute( ctx );

	[FearLevel( 2, "Skip the next normal explore. During the next invader phase, draw an adidtional explore card." )]
	public Task Level2( GameCtx ctx ) {
		var deck = ctx.GameState.InvaderDeck;
		deck.Explore.SkipNextNormal();
		deck.Explore.HoldNextBack();
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
	public Task Level3( GameCtx ctx ) {
		ctx.GameState.AddToAllActiveSpaces(new SkipExploreTo(Name)); // !!! Does this still trigger the escalation ???
		return Task.CompletedTask;
	}

}

sealed public class SkipLowestNumberedExplore : BaseModToken, ISkipExploreTo {

	public SkipLowestNumberedExplore( string label ) : base( label, UsageCost.Free ) {}

	public Task<bool> Skip( GameCtx gameCtx, SpaceState spaceState ) {
		// Remove
		spaceState.Adjust( this, -1 );
		// Find Lowest space
		if(_lowest == null) InitLowest( gameCtx );
		// Return if this is the lowest
		return Task.FromResult( _lowest[spaceState.Space.Board] == spaceState );
	}

	void InitLowest( GameCtx gameCtx ) {
		var card = gameCtx.GameState.InvaderDeck.Explore.Cards.FirstOrDefault();
		_lowest = card == null ? new Dictionary<Board, SpaceState>()
			: gameCtx.GameState.Island.Boards
				.ToDictionary( brd => brd, brd => gameCtx.GameState.Tokens.PowerUp( brd.Spaces ).FirstOrDefault( card.MatchesCard ) );
	}

	Dictionary<Board, SpaceState> _lowest = null;
}