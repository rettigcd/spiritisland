namespace SpiritIsland.JaggedEarth;

public class WeaveTogetherTheFabricOfPlace {

	[MajorCard("Weave Together the Fabric of Place",4, Element.Sun,Element.Moon,Element.Air,Element.Water,Element.Earth), Fast, FromSacredSite(1)]
	[Instructions( "Target land and a land adjacent to it become a single land for this turn. (It has the terrain and land # of both lands. When this effect expires, divide pieces as you wish; all of them are considered moved.) -If you have- 4 Air: Isolate the joined land. If it has Invaders, 2 Fear, and remove up to 2 Invaders." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// target land and a land adjacent to it become a single land for this turn.
		TargetSpaceCtx? otherCtx = (await ctx.SelectAdjacentLandAsync( $"Join {ctx.SpaceSpec.Label} to"));
		if(otherCtx is null) return; // can't weave together if no adjacent land.
		var otherSpec=otherCtx.SpaceSpec;

		Space multi = JoinSpaces( ctx.Self, ctx.SpaceSpec, otherSpec );

		// if you have 4 air:
		if(await ctx.YouHave( "4 air" )) {
			// isolate the joined land.
			var joinedCtx = ctx.TargetSpec( multi.SpaceSpec );
			joinedCtx.Isolate();
			// If it has invaders,
			if(joinedCtx.HasInvaders) {
				// 2 fear,
				await joinedCtx.AddFear( 2 );
				// and Remove up to 2 invaders
				await joinedCtx.Invaders.RemoveLeastDesirable();
				await joinedCtx.Invaders.RemoveLeastDesirable();
			}
		}
	}

	static Space JoinSpaces( Spirit originalSelf, SpaceSpec spaceSpec, SpaceSpec otherSpec ) {

		var multiSpec = MultiSpaceSpec.Build(spaceSpec, otherSpec);

		var gs = GameState.Current;
		Space space = gs.Tokens[spaceSpec];
		Space other = gs.Tokens[otherSpec];
		Space multi = gs.Tokens[multiSpec];

		other.TransferAllTokensTo( multi, true );
		space.TransferAllTokensTo( multi, true );

		// Calculate Adjacents
		List<SpaceSpec> adjacents = spaceSpec.Adjacent_Existing.Union( otherSpec.Adjacent_Existing )
			.Distinct()
			.Where(s=>s!=spaceSpec&&s!=otherSpec)
			.ToList();

		// Disconnect space
		IRestoreable removeSpace = spaceSpec.RemoveFromBoard();
		IRestoreable removeOther = otherSpec.RemoveFromBoard();

		// Add Multi
		multiSpec.AddToBoardsAndSetAdjacent( adjacents );

		ActionScope.Current.Log( new Log.LayoutChanged($"{spaceSpec.Label} and {otherSpec.Label} were woven together") );

		// When this effect expires
		gs.AddTimePassesAction( TimePassesAction.Once( 
			async (gs) => {
				multi.TransferAllTokensTo(space, false); // false => we left the Mod tokens on the original boards and they are still there.
				multiSpec.RemoveFromBoard();
				removeOther.Restore();
				removeSpace.Restore();

				ActionScope.Current.Log( new Log.LayoutChanged( $"{spaceSpec.Label} and {otherSpec.Label} were split up." ) );

				await DistributeVisibleTokens( originalSelf, space, other );

				CopyNewModsToBoth( spaceSpec, otherSpec, multiSpec );
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

	static async Task DistributeVisibleTokens( Spirit self, Space fromSpace, Space toSpace ) {
		await using ActionScope actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,self);

		// Distribute Tokens (All of them are considered moved.)
		ITokenClass[] tokenClasses = fromSpace.OfType<IToken>()
			.Select( x => x.Class ).Distinct()
			.ToArray();
		// await toSpace.Gather(self)
		await new TokenMover(self, $"Distribute tokens to un-woven {toSpace.Label}", fromSpace, toSpace)
			.AddGroup( int.MaxValue, tokenClasses )
			.ConfigSource(s=>s.FilterSource( ss => ss == fromSpace ))
			.DoUpToN();

		// Move remaining onto themselves so they look moved.
		await fromSpace.OfType<IToken>()
			.ToArray()
			.Select( re => re.On(fromSpace).MoveTo(fromSpace) )
			.WhenAll();
	}

}