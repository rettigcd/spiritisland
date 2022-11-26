namespace SpiritIsland;

public static partial class Cmd {

	static public SelfAction DestroyPresence( DestoryPresenceCause actionType ) => new SelfAction("Destroy 1 presence.", ctx => ctx.Presence.DestroyOneFromAnywhere(actionType));

	static public SelfAction ForgetPowerCard => new SelfAction("Forget Power card", ctx => ctx.Self.ForgetPowerCard_UserChoice() );

	static public SelfAction DestroyPresence( int count, DestoryPresenceCause actionType ) => new SelfAction($"Destroy {count} presence", async ctx => {
			for(int i=0;i<count;++i)
				await ctx.Presence.DestroyOneFromAnywhere(actionType);
		}
	);

	#region Pick Land for Action
	static public DecisionOption<SelfCtx> In( this DecisionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "in" );

	static public DecisionOption<SelfCtx> From( this DecisionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "from" );

	static public DecisionOption<SelfCtx> To( this DecisionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "to" );

	// Excludes Oceans
	// !! We could create a version of this that filters on SpaceState instead of the TargetSpaceCtx,
	// but would need to use TerrainMapper in SelfCtx
	// IF, filters depend ONLY on SpaceState
	// !! Before we make this work, we need to fold .IsCostal into SpaceState
	static DecisionOption<SelfCtx> PickLandThenTakeAction( 
		DecisionOption<TargetSpaceCtx> spaceAction,
		Func<TargetSpaceCtx,bool> filter, string filterDescription,
		string landPreposition = "in"  // Depending on Action, "from" or "to" might be better
	)	=> new DecisionOption<SelfCtx>( $"{spaceAction.Description} {landPreposition} {filterDescription}.", 
			async selfCtx => {
				var spaceOptions = selfCtx.GameState.AllActiveSpaces
					.Select(s=>selfCtx.Target(s.Space))
					.Where( x => x.IsInPlay )
					.Where( filter )
					.ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await selfCtx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);

				await spaceAction.Execute(spaceCtx);
			}
		);

	// Excludes Oceans
	static public DecisionOption<SelfCtx> PickDifferentLandThenTakeAction(
		string description,
		DecisionOption<TargetSpaceCtx> spaceAction
	) {
		List<Space> used = new List<Space>();
		return new DecisionOption<SelfCtx>( description, async ctx => {
			var spaceOptions = ctx.GameState.AllActiveSpaces
				.Where( ctx.TerrainMapper.IsInPlay )
				.Select( x => x.Space )
				.Except( used )
				.ToArray();
			if(spaceOptions.Length == 0) return;

			var spaceCtx = await ctx.SelectSpace( "Select space to " + spaceAction.Description, spaceOptions );
			used.Add( spaceCtx.Space );
			await spaceAction.Execute( spaceCtx );
		} );
	}


	#endregion

}