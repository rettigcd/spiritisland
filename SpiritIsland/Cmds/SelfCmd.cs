namespace SpiritIsland;

public static partial class Cmd {

	static public SelfAction DestroyPresence( ActionType actionType ) => new SelfAction("Destroy 1 presence.", ctx => ctx.Presence.DestroyOne(actionType));

	static public SelfAction ForgetPowerCard => new SelfAction("Forget Power card", ctx => ctx.Self.ForgetPowerCard_UserChoice() );

	static public SelfAction DestroyPresence( int count, ActionType actionType ) => new SelfAction("Destroy presence", async ctx => {
			for(int i=0;i<count;++i)
				await ctx.Presence.DestroyOne(actionType);
		}
	);

	#region Pick Land for Action
	static public ActionOption<SelfCtx> In( this ActionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "in" );

	static public ActionOption<SelfCtx> From( this ActionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "from" );

	static public ActionOption<SelfCtx> To( this ActionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> filter, string filterDescription ) 
		=> PickLandThenTakeAction( spaceAction, filter, filterDescription, "to" );

	// Excludes Oceans
	static ActionOption<SelfCtx> PickLandThenTakeAction( 
		ActionOption<TargetSpaceCtx> spaceAction,
		Func<TargetSpaceCtx,bool> filter, string filterDescription,
		string landPreposition = "in"  // Depending on Action, "from" or "to" might be better
	)	=> new ActionOption<SelfCtx>( $"{spaceAction.Description} {landPreposition} {filterDescription}.", 
			async ctx => {
				var spaceOptions = ctx.AllSpaces
					.Select(s=>ctx.Target(s))
					.Where( x => !x.Space.IsOcean && filter(x) )
					.ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
				await spaceAction.Execute(spaceCtx);
			}
		);
	#endregion

	// Excludes Oceans
	static public ActionOption<SelfCtx> PickDifferentLandThenTakeAction(
		string description, 
		ActionOption<TargetSpaceCtx> spaceAction,
		Func<Space,bool> spaceFilter
	) {
		List<Space> used = new List<Space>();
		return new ActionOption<SelfCtx>(description, async ctx => {
			var spaceOptions = ctx.AllSpaces.Where( s => !s.IsOcean && spaceFilter(s) ).Except(used).ToArray();
			if(spaceOptions.Length == 0 ) return;

			var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
			used.Add( spaceCtx.Space );
			await spaceAction.Execute(spaceCtx);
		});
	}
}