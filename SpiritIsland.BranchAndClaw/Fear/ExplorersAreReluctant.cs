namespace SpiritIsland.BranchAndClaw;

public class ExplorersAreReluctant : IFearCard {

	public const string Name = "Explorers are Reluctant";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }


	[FearLevel( 1, "During the next normal explore, skip the lowest-numbered land matching the invader card on each board." )]
	public Task Level1( GameCtx ctx ) {

		Dictionary<Board, Space> lowest = null;
		bool IsLowestMatchingSpace( GameCtx futureCtx, SpaceState ss ) {
			if( lowest == null ) {
				var card = futureCtx.GameState.InvaderDeck.Explore.Cards.FirstOrDefault();
				lowest = card == null ? new Dictionary<Board, Space>()
					: futureCtx.GameState.Island.Boards
						.ToDictionary( brd=>brd, brd=>brd.Spaces.FirstOrDefault(card.MatchesCard));
			}
			return lowest[ss.Space.Board] == ss.Space;
		}

		// this is the next normal build
		ctx.GameState.AddToAllActiveSpaces( new SkipExploreTo_Custom(Name,false, IsLowestMatchingSpace) );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Skip the next normal explore. During the next invader phase, draw an adidtional explore card." )]
	public Task Level2( GameCtx ctx ) {
		var deck = ctx.GameState.InvaderDeck;
		deck.Explore.SkipNextNormal();
		deck.Explore.HoldNextBack();
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
	public Task Level3( GameCtx ctx ) {
		ctx.GameState.AddToAllActiveSpaces(new SkipExploreTo(Name));
		return Task.CompletedTask;
	}

}