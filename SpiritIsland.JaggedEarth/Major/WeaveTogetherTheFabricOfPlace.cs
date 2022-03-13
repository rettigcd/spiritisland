namespace SpiritIsland.JaggedEarth;

public class WeaveTogetherTheFabricOfPlace {

	[MajorCard("Weave Together the Fabric of Place",4, Element.Sun,Element.Moon,Element.Air,Element.Water,Element.Earth), Fast, FromSacredSite(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// target land and a land adjacent to it become a single land for this turn.
		var other = (await ctx.SelectAdjacentLand( $"Join {ctx.Space.Label} to." )).Space;

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
				await joinedCtx.Invaders.Remove();
				await joinedCtx.Invaders.Remove();
			}
		}
	}

	private static MultiSpace JoinSpaces( SelfCtx originatorCtx,Space space, Space other ) {

		var gameState = originatorCtx.GameState;

		var multi = new MultiSpace( space, other );
		MoveAllItemsOnSpace( gameState, other, multi );
		MoveAllItemsOnSpace( gameState, space, multi );

		// Pick Board
		var board = space.Board; // !!! this is not correct when we start having multiple boards.

		// Disconnect space
		var spaceAdjacents = board.Remove( space );
		var otherAdjacents = board.Remove( other );
		// it has the terrain and land # of both lands.
		board.Add( multi, spaceAdjacents.Union( otherAdjacents ).Distinct().ToArray() );

		// When this effect expires
		gameState.TimePasses_ThisRound.Push( async (gs) => {

			MoveAllItemsOnSpace( gs, multi, space );
			board.Remove( multi );
			board.Add( other, otherAdjacents );
			board.Add( space, spaceAdjacents );

			// divide pieces as you wish.
			await DistributePresence( space, other, gs );
			await DistributeTokens( originatorCtx, space, other, gs );
		});

		return multi;
	}

	static async Task DistributeTokens( SelfCtx ctx, Space space, Space other, GameState gs ) {
		// Distribute Tokens (All of them are considered moved.)
		// !!! not counting unmoved tokens as moved li
		var srcTokens = gs.Tokens[space];
		while(srcTokens.Keys.Any()) {
			var tokenToMove = await ctx.Decision( Select.TokenFrom1Space.TokenToPush( space, 100, srcTokens.Keys.ToArray(), Present.Done ) );
			if(tokenToMove == null) break;
			await srcTokens.MoveTo( tokenToMove, other );
		}
	}

	static async Task DistributePresence( Space space, Space other, GameState gs ) {
		var dstOptions = new[] { other };
		foreach(var spirit in gs.Spirits) {
			int count = spirit.Presence.CountOn( space );
			while(count > 0) {
				var spiritCtx = spirit.Bind( gs, Cause.Power );
				var dst = await spiritCtx.Decision( Select.Space.ForAdjacent( "Distribute preseence to:", space, Select.AdjacentDirection.Outgoing, dstOptions, Present.Done ) );
				if(dst == null) break;
				spiritCtx.Presence.Move( space, other );
				--count;
			}
		}
	}

	static void MoveAllItemsOnSpace(GameState gs, Space src, Space dst ) {
		var dstTokens = gs.Tokens[dst];
		var srcTokens = gs.Tokens[src];
		foreach(var token in srcTokens.Keys.ToArray()) {
			int count = srcTokens[token];
			srcTokens.Adjust(token, -count);
			dstTokens.Adjust(token, count);
		}
		foreach(var spirit in gs.Spirits) {
			var presence = spirit.Presence;
			int count = presence.CountOn(src);
			presence.Adjust(src, -count);
			presence.Adjust(dst, count);
		}
	}

}