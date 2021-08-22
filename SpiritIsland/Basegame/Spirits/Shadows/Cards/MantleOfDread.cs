using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	class MantleOfDread {

		[SpiritCard("Mantle of Dread",1,Speed.Slow,Element.Moon,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task Act( TargetSpiritCtx ctx ){

			var gs = ctx.GameState;

			// 2 fear
			ctx.GameState.AddFearDirect(new FearArgs{ count=2,cause=Cause.Power,space=null }); // not associated with any space

			// target spirit may push 1 explorer and 1 town from land where it has presence
			bool HasExplorerOrTown(Space space){
				var grp = gs.InvadersOn(space);
				return grp.HasExplorer || grp.HasTown;
			}
			// Select Land
			var landsToPushInvadersFrom = ctx.Target.Presence.Spaces.Where(HasExplorerOrTown).ToArray();
			if(landsToPushInvadersFrom.Length == 0) return;

			var otherSpirit = new PowerCtx(ctx.Target,ctx.GameState);

			var space = await otherSpirit.Self.SelectSpace("Select land to push 1 exploer & 1 town from",landsToPushInvadersFrom,Present.Done);
			if(space==null) return;

			// Push Town
			await otherSpirit.PowerPushUpToNInvaders( space, 1, Invader.Town );
			// Push Explorer
			await otherSpirit.PowerPushUpToNInvaders( space, 1, Invader.Explorer );
			
		}

	}

}
