using System;
using System.Linq;

namespace SpiritIsland {
	public static class BoardCmd {

		static public ActionOption<BoardCtx> PickSpaceThenTakeAction(string description, ActionOption<TargetSpaceCtx> spaceAction,Func<Space,bool> spaceFilter)
			=> new ActionOption<BoardCtx>(description, async ctx => {
				var spaceOptions = ctx.Board.Spaces.Where( spaceFilter ).ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
				await spaceAction.Execute(spaceCtx);
			});

	}

}
