namespace SpiritIsland.JaggedEarth;

public class WeaveTogetherTheFabricOfPlace {

	[MajorCard("Weave Together the Fabric of Place",4, Element.Sun,Element.Moon,Element.Air,Element.Water,Element.Earth), Fast, FromSacredSite(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// target land and a land adjacent to it become a single land for this turn.
		var other = (await ctx.SelectAdjacentLand( $"Join {ctx.Space.Label} to.")).Space;

		MultiSpace multi = JoinSpaces( ctx, ctx.Space, other );

		// if you have 4 air:
		if(await ctx.YouHave( "4 air" )) {
			// isolate the joined land.
			var joinedCtx = ctx.Target( multi );
			joinedCtx.Isolate();
			// If it has invaders,
			if(joinedCtx.HasInvaders) {
				// 2 fear,
				joinedCtx.AddFear( 2 );
				// and Remove up to 2 invaders
				await joinedCtx.Invaders.RemoveLeastDesirable();
				await joinedCtx.Invaders.RemoveLeastDesirable();
			}
		}
	}

	static MultiSpace JoinSpaces( SelfCtx originatorCtx, Space space, Space other ) {

		var gameState = originatorCtx.GameState;

		var multi = new MultiSpace( space, other );
		MoveAllItemsOnSpace( gameState, other, multi );
		MoveAllItemsOnSpace( gameState, space, multi );

		// Calculate Adjacents
		var adjacents = space.Adjacent.Union( other.Adjacent ).Distinct().Where(s=>s!=space&&s!=other).ToList();

		// Disconnect space
		Restoreable removeSpace = space.RemoveFromBoard();
		Restoreable removeOther = other.RemoveFromBoard();

		// Add Multi
		multi.AddToBoards( adjacents.Distinct() );

		// !!! Any game-wide mod-tokens are going to be put on next space, but will be difficult to split since they aren't visible

		gameState.Log( new Log.LayoutChanged($"{space.Text} and {other.Text} were woven together") );

		// When this effect expires
		gameState.TimePasses_ThisRound.Push( async (gs) => {

			MoveAllItemsOnSpace( gs, multi, space );
			multi.RemoveFromBoard();
			removeOther.Restore();
			removeSpace.Restore();

			gameState.Log( new Log.LayoutChanged( $"{space.Text} and {other.Text} were split up." ) );

			// divide pieces as you wish.
			await using UnitOfWork actionScope = gs.StartAction( ActionCategory.Spirit_Power );
			await DistributeTokens( originatorCtx, space, other, gs );
		});

		return multi;
	}

	static async Task DistributeTokens( SelfCtx ctx, Space space, Space other, GameState gs ) {
		// Distribute Tokens (All of them are considered moved.)
		var tokens = gs.Tokens[space].Keys.OfType<IToken>().ToArray();
		var tokenClasses = tokens.Select( x => x.Class ).Distinct().ToArray();
		await new TokenGatherer( ctx.Target( other ) )
			.AddGroup( int.MaxValue, tokenClasses )
			.FilterSource( ss => ss.Space == space )
			.GatherUpToN();
		// !!! Remove / Add un-moved tokens so they act like they were moved.
		// !!! Divy up invisible mod tokens.
	}

	static void MoveAllItemsOnSpace(GameState gs, Space src, Space dst ) {
		var srcTokens = gs.Tokens[src];
		var dstTokens = gs.Tokens[dst];
		foreach(var token in srcTokens.Keys.ToArray()) {
			int count = srcTokens[token];
			srcTokens.Adjust(token, -count);
			dstTokens.Adjust(token, count);
		}
		foreach(var spirit in gs.Spirits) {
			var presence = spirit.Presence;
			int count = presence.CountOn( srcTokens );
			srcTokens.Adjust( presence.Token, -count);
			dstTokens.Adjust( presence.Token, count );
		}
	}

}