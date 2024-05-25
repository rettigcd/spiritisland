namespace SpiritIsland.JaggedEarth;

public class WeaveTogetherTheFabricOfPlace {

	[MajorCard("Weave Together the Fabric of Place",4, Element.Sun,Element.Moon,Element.Air,Element.Water,Element.Earth), Fast, FromSacredSite(1)]
	[Instructions( "Target land and a land adjacent to it become a single land for this turn. (It has the terrain and land # of both lands. When this effect expires, divide pieces as you wish; all of them are considered moved.) -If you have- 4 Air: Isolate the joined land. If it has Invaders, 2 Fear, and remove up to 2 Invaders." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// target land and a land adjacent to it become a single land for this turn.
		var other = (await ctx.SelectAdjacentLandAsync( $"Join {ctx.SpaceSpec.Label} to.")).SpaceSpec;

		MultiSpaceSpec multi = JoinSpaces( ctx.Self, ctx.SpaceSpec, other );

		// if you have 4 air:
		if(await ctx.YouHave( "4 air" )) {
			// isolate the joined land.
			var joinedCtx = ctx.TargetSpec( multi );
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

	static MultiSpaceSpec JoinSpaces( Spirit originalSelf, SpaceSpec space, SpaceSpec other ) {

		var gameState = GameState.Current;

		var multi = new MultiSpaceSpec( space, other );
		MoveItemsOnSpace( other, multi, true );
		MoveItemsOnSpace( space, multi, true );

		// Calculate Adjacents
		List<SpaceSpec> adjacents = space.Adjacent_Existing.Union( other.Adjacent_Existing ).Distinct().Where(s=>s!=space&&s!=other).ToList();

		// Disconnect space
		IRestoreable removeSpace = space.RemoveFromBoard();
		IRestoreable removeOther = other.RemoveFromBoard();

		// Add Multi
		multi.AddToBoardsAndSetAdjacent( adjacents.Distinct() );

		ActionScope.Current.Log( new Log.LayoutChanged($"{space.Label} and {other.Label} were woven together") );

		// When this effect expires
		gameState.AddTimePassesAction( TimePassesAction.Once( 
			async (gs) => {
				MoveItemsOnSpace( multi, space, false );
				multi.RemoveFromBoard();
				removeOther.Restore();
				removeSpace.Restore();

				ActionScope.Current.Log( new Log.LayoutChanged( $"{space.Label} and {other.Label} were split up." ) );

				await DistributeVisibleTokens( originalSelf, space, other );

				CopyNewModsToBoth( space, other, multi );
			}
		) );

		return multi;
	}

	static void CopyNewModsToBoth( SpaceSpec space, SpaceSpec other, MultiSpaceSpec multi ) {
		var a = space.ScopeSpace;
		var b = other.ScopeSpace;
		var newMods = multi.ScopeSpace.Keys.Where( k => k is not IToken )
			.Except( a.Keys )
			.Except( b.Keys )
			.ToArray();
		foreach(var x in newMods) {
			a.Init( x, 1 );
			b.Init( x, 1 );
		}
	}

	static async Task DistributeVisibleTokens( Spirit self, SpaceSpec from, SpaceSpec to ) {
		await using ActionScope actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,self);

		Space fromSpace = from.ScopeSpace;
		Space toSpace = to.ScopeSpace;

		// Distribute Tokens (All of them are considered moved.)
		ITokenClass[] tokenClasses = fromSpace.OfType<IToken>()
			.Select( x => x.Class ).Distinct()
			.ToArray();
		await toSpace.Gather( self )
			.AddGroup( int.MaxValue, tokenClasses )
			.ConfigSource(s=>s.FilterSource( ss => ss.SpaceSpec == from ))
			.DoUpToN();

		// Move remaining onto themselves so they look moved.
		await fromSpace.OfType<IToken>()
			.ToArray()
			.Select( re => re.MoveAsync(fromSpace, fromSpace) )
			.WhenAll();
	}

	static void MoveItemsOnSpace( SpaceSpec src, SpaceSpec dst, bool copyInvisible ) {
		var srcTokens = src.ScopeSpace;
		var dstTokens = dst.ScopeSpace;
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