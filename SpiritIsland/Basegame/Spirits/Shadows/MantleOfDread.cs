using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	class MantleOfDread {

		[SpiritCard("Mantle of Dread",1,Speed.Slow,Element.Moon,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task Act( IMakeGamestateDecisions ctx, Spirit target ){

			var gs = ctx.GameState;

			// 2 fear
			ctx.AddFear(2);

			// target spirit may push 1 explorer and 1 town from land where it has presence
			bool HasExplorerOrTown(Space space){
				var grp = gs.InvadersOn(space);
				return grp.HasExplorer || grp.HasTown;
			}
			// Select Land
			var landsToPushInvadersFrom = target.Presence.Spaces.Where(HasExplorerOrTown).ToArray();
			if(landsToPushInvadersFrom.Length == 0) return;
			var space = await ctx.Self.SelectSpace("Select land to push 1 exploer & 1 town from",landsToPushInvadersFrom,true);
			if(space==null) return;

			// Push Town
			await ctx.PushUpToNInvaders( space, 1, Invader.Town );
			// Push Explorer
			await ctx.PushUpToNInvaders( space, 1, Invader.Explorer );
			
		}

	}

}
