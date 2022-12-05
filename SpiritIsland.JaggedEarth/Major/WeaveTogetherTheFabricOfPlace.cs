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
				await joinedCtx.Invaders.RemoveLeastDesirable();
				await joinedCtx.Invaders.RemoveLeastDesirable();
			}
		}
	}

	private static MultiSpace JoinSpaces( SelfCtx originatorCtx, Space space, Space other ) {

		var gameState = originatorCtx.GameState;

		var multi = new MultiSpace( space, other );
		MoveAllItemsOnSpace( gameState, other, multi );
		MoveAllItemsOnSpace( gameState, space, multi );

		// Calculate Adjacents
		var adjacents = space.Adjacent.Union( other.Adjacent ).Distinct().Where(s=>s!=space&&s!=other).ToList();

		// Disconnect space
		var removeSpace = space.RemoveFromBoard();
		var removeOther = other.RemoveFromBoard();

		// Add Multi
		multi.AddToBoards( adjacents.Distinct() );

		// When this effect expires
		gameState.TimePasses_ThisRound.Push( async (gs) => {

			MoveAllItemsOnSpace( gs, multi, space );
			multi.RemoveFromBoard();
			removeOther.Restore();
			removeSpace.Restore();

			// divide pieces as you wish.
			UnitOfWork actionId = gs.StartAction( ActionCategory.Spirit );
			await DistributePresence( space, other, gs, actionId );
			await DistributeTokens( originatorCtx, space, other, gs, actionId );
		});

		return multi;
	}

	static async Task DistributeTokens( SelfCtx ctx, Space space, Space other, GameState gs, UnitOfWork actionId ) {
		// Distribute Tokens (All of them are considered moved.)
		// !!! not counting unmoved tokens as moved li
		var srcTokens = gs.Tokens[space];
		while(srcTokens.Keys.Any()) {
			Token tokenToMove = await ctx.Decision( Select.TokenFrom1Space.TokenToPush( space, 100, srcTokens.Keys.ToArray(), Present.Done ) );
			if(tokenToMove == null) break;
			await ctx.Move( tokenToMove, space, other );
		}
	}

	static async Task DistributePresence( Space space, Space other, GameState gs, UnitOfWork actionId ) {
		var dstOptions = new[] { gs.Tokens[other] };
		var srcTokens = gs.Tokens[space];
		foreach(var spirit in gs.Spirits) {
			int count = spirit.Presence.CountOn( srcTokens );
			while(count > 0) {
				var dst = await spirit.Gateway.Decision( Select.Space.ForAdjacent( "Distribute preseence to:", space, Select.AdjacentDirection.Outgoing, dstOptions, Present.Done ) );
				if(dst == null) break;
				await new BoundPresence( spirit, gs, gs.Island.Terrain_ForPower, actionId ).Move( space, other );
				--count;
			}
		}
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
			presence.Adjust( srcTokens, -count);
			presence.Adjust( dstTokens, count);
		}
	}

}