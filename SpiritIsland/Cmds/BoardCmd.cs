namespace SpiritIsland;

using BoardCmd = DecisionOption<BoardCtx>;

public static partial class Cmd {

	#region Convert a TargetSpaceCtx command to a BoardCtx command.

	// Given a space-command, return a board-command that allows user to pick a space then execute space-command on it.

	static public BoardCmd FromLandOnBoard( this DecisionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> spaceFilter, string filterDescription )
		=> PickLandOnBoardThenTakeAction( spaceAction, spaceFilter, "from " + filterDescription );

	static public BoardCmd ToLandOnBoard( this DecisionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> spaceFilter, string filterDescription )
		=> PickLandOnBoardThenTakeAction( spaceAction, spaceFilter, "to " + filterDescription );

	static public BoardCmd InAnyLandOnBoard( this DecisionOption<TargetSpaceCtx> spaceAction )
		=> PickLandOnBoardThenTakeAction( spaceAction, _=>true, "" ); // Replace this with a Filter that wraps true,"" ie.  Filters.AnyLand

	static BoardCmd PickLandOnBoardThenTakeAction(
		this DecisionOption<TargetSpaceCtx> spaceAction,
		Func<TargetSpaceCtx,bool> customFilter,
		string filterDescription
	)
		=> new BoardCmd( spaceAction.Description + " " + filterDescription, async ctx => {

			// !!! although filtering by the Action Criteria is helpful most of the time,
			// There might be instances when players wants to pick the no-op action.
			// Therefore we might not want to filter based on the action.
			// Maybe pass both spaces to UI so UI can determine which options to make available.

			var spaceOptions = ctx.Board.Spaces
				.Select( ctx.Target )
				.Where( x => x.IsInPlay )			// layer 2 filter
				.Where( spaceAction.IsApplicable )	// Matches action criteria  (Can't act on items that aren't there)
				.Where( customFilter )				// Matches custom space - criteria
				.ToArray();

			if(spaceOptions.Length == 0 ) return;

			var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
			await spaceAction.Execute(spaceCtx);
		});

	static public BoardCmd InNDifferentLands( 
		this SpaceAction action,
		int count, 
		Func<TargetSpaceCtx, bool> customFilter
	)
		=> new BoardCmd( $"In {count} different lands, " + action.Description, async ctx => {
			var spaceOptions = ctx.Board.Spaces
				.Where( ctx.TerrainMapper.IsInPlay )
				.Select( ctx.Target )
				.Where( action.IsApplicable )	// Matches action criteria  (Can't act on items that aren't there)
				.Where( customFilter )			// Matches custom space - criteria
				.ToList();
			for(int i = 0; i < count; ++i) {
				var spaceCtx = await ctx.SelectSpace( action.Description, spaceOptions );
				if(spaceCtx == null) continue;
				spaceOptions.Remove( spaceCtx );
				await action.Execute( spaceCtx );
			}
		} );

	#endregion

	#region Convert a TargetSpaceCtx command to a SelfCtx command

	// Allow spirit to take action in any land.
	static public DecisionOption<SelfCtx> InAnyLand( this DecisionOption<TargetSpaceCtx> spaceAction )
		=> PickLandThenTakeAction( spaceAction, _ => true, "" ); // Replace this with a Filter that wraps true,"" ie.  Filters.AnyLand

	static DecisionOption<SelfCtx> PickLandThenTakeAction(
		this DecisionOption<TargetSpaceCtx> spaceAction,
		Func<TargetSpaceCtx, bool> customFilter, // !!! change to TargetSpaceCtx
		string filterDescription
	)
		=> new DecisionOption<SelfCtx>( spaceAction.Description + " " + filterDescription, async ctx => {

			var spaceOptions = ctx.GameState.AllActiveSpaces
				.Select( x=>ctx.Target(x.Space) )
				.Where( x => x.IsInPlay )
				.Where( spaceAction.IsApplicable )  // Matches action criteria  (Can't act on items that aren't there)
				.Where( customFilter )              // Matches custom space - criteria
				.ToArray();

			if(spaceOptions.Length == 0) return;

			var spaceCtx = await ctx.SelectSpace( "Select space to " + spaceAction.Description, spaceOptions );
			await spaceAction.Execute( spaceCtx );
		} );

	#endregion

}