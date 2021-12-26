using System;
using System.Linq;

namespace SpiritIsland {
	public static class SelfCmd {

		static public SelfAction DestoryPresence( ActionType actionType ) => new SelfAction("Destory presence", ctx => ctx.Presence.DestoryOne(actionType));

		static public SelfAction DestoryPresence( int count, ActionType actionType ) => new SelfAction("Destory presence", async ctx => {
				for(int i=0;i<count;++i)
					await ctx.Presence.DestoryOne(actionType);
			}
		);

		static public ActionOption<SelfCtx> PickSpaceThenTakeAction(string description, ActionOption<TargetSpaceCtx> spaceAction,Func<Space,bool> spaceFilter)
			=> new ActionOption<SelfCtx>(description, async ctx => {
				var spaceOptions = ctx.AllSpaces.Where( spaceFilter ).ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
				await spaceAction.Execute(spaceCtx);
			});


	}

}
