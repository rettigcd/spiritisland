namespace SpiritIsland;

using BoardCmd = ActionOption<BoardCtx>;

public static partial class Cmd {

	static public BoardCmd FromLandOnBoard( this ActionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> spaceFilter, string filterDescription )
		=> PickLandOnBoardThenTakeAction( spaceAction, spaceFilter, "from " + filterDescription );

	static public BoardCmd ToLandOnBoard( this ActionOption<TargetSpaceCtx> spaceAction, Func<TargetSpaceCtx,bool> spaceFilter, string filterDescription )
		=> PickLandOnBoardThenTakeAction( spaceAction, spaceFilter, "to " + filterDescription );

	static public BoardCmd InAnyLandOnBoard( this ActionOption<TargetSpaceCtx> spaceAction )
		=> PickLandOnBoardThenTakeAction( spaceAction, _=>true, "" ); // Replace this with a Filter that wraps true,"" ie.  Filters.AnyLand

	static BoardCmd PickLandOnBoardThenTakeAction(
		this ActionOption<TargetSpaceCtx> spaceAction,
		Func<TargetSpaceCtx,bool> customFilter, // !!! change to TargetSpaceCtx
		string filterDescription
	)
		=> new BoardCmd( spaceAction.Description + " " + filterDescription, async ctx => {

			// !!! although filtering by the Action Criteria is helpful most of the time,
			// There might be instances when players wants to pick the no-op action.
			// Therefore we might not want to filter based on the action.
			// Maybe pass both spaces to UI so UI can determine which options to make available.

			var spaceOptions = ctx.Board.Spaces
				.Where(s => !s.IsOcean )			// skip Ocean
				.Select( ctx.Target )
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
				.Where(s => !s.IsOcean ) // skip Ocean
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


}