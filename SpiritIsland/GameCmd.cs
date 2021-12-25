using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public static class GameCmd {
		// GameState actions
		static public ActionOption<GameState> OnEachBoard( ActionOption<BoardCtx> boardAction ) 
			=> new ActionOption<GameState>( "On each board, " + boardAction.Description, async gs => {
				for(int i = 0; i < gs.Spirits.Length; ++i) {
					BoardCtx boardCtx = new BoardCtx( gs.Spirits[i], gs, gs.Island.Boards[i] );
					await boardAction.Execute( boardCtx );
				}
			} );

		static public ActionOption<GameState> InEachLand( SpaceAction action, Func<Space,bool> filter, Cause cause )
			=> new ActionOption<GameState>("In each land, " + action.Description, async gs => {
				foreach(var spiritCtx in gs.SpiritCtxs(cause)) {
					var spaceOptions = gs.Island.AllSpaces
						.Where( filter )
						.ToArray();
					var spaceCtx = await spiritCtx.SelectSpace( action.Description, spaceOptions );
					if(spaceCtx == null) continue;
					await action.Execute(spaceCtx);
				}
			} );

		static public ActionOption<GameState> EachSpirit( Cause cause, ActionOption<SelfCtx> action )
			=> new ActionOption<GameState>("For each spirit, " + action.Description, async gs => {
				foreach(var spiritCtx in gs.SpiritCtxs(cause) )
					await action.Execute( spiritCtx );
			});

		static public ActionOption<GameState> EachPlayerTakesActionInALand( SpaceAction action, Func<TargetSpaceCtx,bool> filter, Cause cause, bool differentLands = false )
			=> new ActionOption<GameState>(
				"Each player selects a land. In that land, " + action.Description, async gs => {
					var used = new List<Space>();
					foreach(var spiritCtx in gs.SpiritCtxs(cause)) {
						var spaceOptions = gs.Island.AllSpaces
							.Where(s=>filter(spiritCtx.Target(s)))
							.Except(used)
							.ToArray();
						var spaceCtx = await spiritCtx.SelectSpace( action.Description, spaceOptions );
						if(spaceCtx == null) continue;
						if(differentLands)
							used.Add( spaceCtx.Space );
						await action.Execute(spaceCtx);
					}
				}
			);
	}

}
