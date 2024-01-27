namespace SpiritIsland.JaggedEarth;

public class WeaveTogetherTheFabricOfPlace {

	[MajorCard("Weave Together the Fabric of Place",4, Element.Sun,Element.Moon,Element.Air,Element.Water,Element.Earth), Fast, FromSacredSite(1)]
	[Instructions( "Target land and a land adjacent to it become a single land for this turn. (It has the terrain and land # of both lands. When this effect expires, divide pieces as you wish; all of them are considered moved.) -If you have- 4 Air: Isolate the joined land. If it has Invaders, 2 Fear, and remove up to 2 Invaders." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// target land and a land adjacent to it become a single land for this turn.
		var other = (await ctx.SelectAdjacentLandAsync( $"Join {ctx.Space.Label} to.")).Space;

		MultiSpace multi = JoinSpaces( ctx.Self, ctx.Space, other );

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

	static MultiSpace JoinSpaces( Spirit originalSelf, Space space, Space other ) {

		var gameState = GameState.Current;

		var multi = new MultiSpace( space, other );
		MoveItemsOnSpace( other, multi, true );
		MoveItemsOnSpace( space, multi, true );

		// Calculate Adjacents
		var adjacents = space.Adjacent_Existing.Union( other.Adjacent_Existing ).Distinct().Where(s=>s!=space&&s!=other).ToList();

		// Disconnect space
		Restoreable removeSpace = space.RemoveFromBoard();
		Restoreable removeOther = other.RemoveFromBoard();

		// Add Multi
		multi.AddToBoardsAndSetAdjacent( adjacents.Distinct() );

		ActionScope.Current.Log( new Log.LayoutChanged($"{space.Text} and {other.Text} were woven together") );

		// When this effect expires
		gameState.AddTimePassesAction( TimePassesAction.Once( 
			async (gs) => {
				MoveItemsOnSpace( multi, space, false );
				multi.RemoveFromBoard();
				removeOther.Restore();
				removeSpace.Restore();

				ActionScope.Current.Log( new Log.LayoutChanged( $"{space.Text} and {other.Text} were split up." ) );

				await DistributeVisibleTokens( originalSelf, space, other );

				CopyNewModsToBoth( space, other, multi );
			}
		) );

		return multi;
	}

	static void CopyNewModsToBoth( Space space, Space other, MultiSpace multi ) {
		var a = space.Tokens;
		var b = other.Tokens;
		var newMods = multi.Tokens.Keys.Where( k => k is not IToken )
			.Except( a.Keys )
			.Except( b.Keys )
			.ToArray();
		foreach(var x in newMods) {
			a.Init( x, 1 );
			b.Init( x, 1 );
		}
	}

	static async Task DistributeVisibleTokens( Spirit self, Space from, Space to ) {
		await using ActionScope actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,self);

		var fromTokens = from.Tokens;
		var toTokens = to.Tokens;

		// Distribute Tokens (All of them are considered moved.)
		var tokenClasses = fromTokens.OfType<IToken>()
			.Select( x => x.Class ).Distinct()
			.ToArray();
		await toTokens.Gather( self )
			.AddGroup( int.MaxValue, tokenClasses )
			.ConfigSource(s=>s.FilterSource( ss => ss.Space == from ))
			.DoUpToN();

		// Move remaining onto themselves so they look moved.
		await fromTokens.OfType<IToken>()
			.ToArray()
			.Select( re => re.MoveAsync(from,from) )
			.WhenAll();
	}

	static void MoveItemsOnSpace( Space src, Space dst, bool copyInvisible ) {
		var srcTokens = src.Tokens;
		var dstTokens = dst.Tokens;
		foreach(var key in srcTokens.Keys.ToArray()) {
			int count = srcTokens[key];
			if(key is IToken) {
				// move visible
				srcTokens.Adjust(key, -count);
				dstTokens.Adjust(key, count);
			} else if( copyInvisible )
				// copy invisible (orig keep their invisible mods)
				dstTokens.Adjust(key, count);
		}
	}

}