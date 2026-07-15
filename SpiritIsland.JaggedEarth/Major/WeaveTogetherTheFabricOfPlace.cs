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
		var removeSpace = (SpaceSpec.DisconnectSpaceResults)spaceSpec.RemoveFromBoard();
		var removeOther = (SpaceSpec.DisconnectSpaceResults)otherSpec.RemoveFromBoard();

		// Add Multi
		multiSpec.AddToBoardsAndSetAdjacent( adjacents );

		ActionScope.Current.Log( new Log.LayoutChanged($"{spaceSpec.Label} and {otherSpec.Label} were woven together") );

		// When this effect expires
		gs.AddTimePassesAction( new UnweaveSpaces( originalSelf, spaceSpec, removeSpace.OldAdjacents, otherSpec, removeOther.OldAdjacents ) );

		return multi;
	}

	/// <summary>
	/// Replaces the old TimePassesAction.Once(...) closure - docs/GameSerialization-Roadmap.md's hook
	/// action lists section. Most of what the closure captured (multi/space/other/multiSpec) is cheaply
	/// re-derivable from spaceSpec/otherSpec alone (MultiSpaceSpec.Build is deterministic, gs.Tokens[spec]
	/// is just a view) - the only real state is the Spirit plus each side's identity and OldAdjacents.
	///
	/// Identity resolution: while the weave is active, spaceSpec/otherSpec are detached from their
	/// Board's own space list (RemoveFromBoard took them out), so ctx.SpaceSpecByLabel can't find them
	/// directly. The *joined* space is still normally attached though, so it resolves fine - and
	/// MultiSpaceSpec.OrigSpaces on that live object already holds direct references to the original,
	/// fully-valid SingleSpaceSpecs. Serializing each side as its set of underlying single-space labels
	/// (rather than assuming exactly one) also covers re-weaving an already-merged land: if a side has
	/// more than one label, it's rebuilt via MultiSpaceSpec.Build from the matching OrigSpaces entries.
	/// </summary>
	internal class UnweaveSpaces( Spirit self, SpaceSpec spaceSpec, SpaceSpec[] spaceOldAdjacents, SpaceSpec otherSpec, SpaceSpec[] otherOldAdjacents )
		: IRunWhenTimePasses {

		bool IRunWhenTimePasses.RemoveAfterRun => true;
		TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

		async Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
			var multiSpec = MultiSpaceSpec.Build( spaceSpec, otherSpec );
			Space space = gameState.Tokens[spaceSpec];
			Space other = gameState.Tokens[otherSpec];
			Space multi = gameState.Tokens[multiSpec];

			multi.TransferAllTokensTo( space, false ); // false => we left the Mod tokens on the original boards and they are still there.
			multiSpec.RemoveFromBoard();
			new SpaceSpec.DisconnectSpaceResults { Space = otherSpec, OldAdjacents = otherOldAdjacents }.Restore();
			new SpaceSpec.DisconnectSpaceResults { Space = spaceSpec, OldAdjacents = spaceOldAdjacents }.Restore();

			ActionScope.Current.Log( new Log.LayoutChanged( $"{spaceSpec.Label} and {otherSpec.Label} were split up." ) );

			await DistributeVisibleTokens( self, space, other );

			CopyNewModsToBoth( spaceSpec, otherSpec, multiSpec );
		}

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
			.UseQuota(new Quota().AddGroup( int.MaxValue, tokenClasses ))
			.ConfigSource(s=>s.FilterSource( ss => ss == fromSpace ))
			.DoUpToN();

		// Move remaining onto themselves so they look moved.
		await fromSpace.OfType<IToken>()
			.ToArray()
			.Select( re => re.On(fromSpace).MoveTo(fromSpace) )
			.WhenAll();
	}

}