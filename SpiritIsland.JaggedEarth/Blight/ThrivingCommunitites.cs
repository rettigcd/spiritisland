using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ThrivingCommunitites : BlightCardBase {

		public ThrivingCommunitites():base("Thriving Communitites",4) {}

		protected override Task BlightAction( GameState gs ) {
			// Immediately,
			// on each board:
			return GameCmd.OnEachBoard(
				// In 4 different lands with explorer/town,
				BoardCmd.PickNDifferentSpacesThen(
					4,
					// Replace 1 town with a city or Replace 1 explorer with 1 town
					UpgradeExplorerOrTown, 
					tokens => tokens.HasAny(Invader.Explorer,Invader.Town)
				)
			).Execute( gs );
		}

		static SpaceAction UpgradeExplorerOrTown => new SpaceAction( "Replace 1 town with a city or Replace 1 explorer with 1 town", async ctx=>{
			var invader = await ctx.Decision( Select.Invader.ToDowngrade("upgrade",ctx.Space,ctx.Tokens.OfAnyType(Invader.Explorer,Invader.Town)));
			await ctx.Tokens.Remove(invader,1,RemoveReason.Replaced);
			// !!! if select invader with strife, we should keep the strife.
			var newToken = (invader.Class == Invader.Explorer)
				? Invader.Town.Default
				: Invader.City.Default;
			await ctx.Tokens.Add( newToken, 1, AddReason.AsReplacement );
		});

	}

}
