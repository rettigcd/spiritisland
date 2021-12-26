using System;
using System.Linq;

namespace SpiritIsland {

	public static class BoardCmd {

		static public ActionOption<BoardCtx> PickSpaceThenTakeAction(
			string description, 
			ActionOption<TargetSpaceCtx> spaceAction,
			Func<TokenCountDictionary,bool> spaceFilter
		)
			=> new ActionOption<BoardCtx>(description, async ctx => {
				var spaceOptions = ctx.Board.Spaces.Where( s => spaceFilter( ctx.GameState.Tokens[s] ) ).ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
				await spaceAction.Execute(spaceCtx);
			});

		static public ActionOption<BoardCtx> PickNDifferentSpacesThen( 
			int count, 
			SpaceAction action,
			Func<TokenCountDictionary, bool> filter
		)
			=> new ActionOption<BoardCtx>( $"In {count} different lands, " + action.Description, async ctx => {
				var spaceOptions = ctx.Board.Spaces
					.Where( s => filter( ctx.GameState.Tokens[s] ) )
					.ToList();
				for(int i = 0; i < count; ++i) {
					var spaceCtx = await ctx.SelectSpace( action.Description, spaceOptions );
					if(spaceCtx == null) continue;
					spaceOptions.Remove( spaceCtx.Space );
					await action.Execute( spaceCtx );
				}
			} );


	}

}
