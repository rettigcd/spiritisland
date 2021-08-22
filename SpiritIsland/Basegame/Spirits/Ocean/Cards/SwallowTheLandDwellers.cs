using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SwallowTheLandDwellers {

		[SpiritCard("Swallow the Land-Dwellers",0,Speed.Slow,Element.Water,Element.Earth)]
		[FromPresence(0,Target.Costal)]
		static public async Task Act(TargetSpaceCtx ctx ) {

			// find Ocean
			var ocean = ctx.Self as Ocean ?? ctx.GameState.Spirits.Single(x=>x is Ocean);
			// find place to drown then
			var drowningOcean = ocean.Presence.Spaces.First().Board[0]; //

			// drown 1 explorer, 1 town, and 1 dahan

			// drop explorer in the ocean to drown
			if( ctx.Invaders.HasExplorer)
				await ctx.GameState.MoveInvader(InvaderSpecific.Explorer,ctx.Target,drowningOcean);

			// drop town in the ocean to drown
			if(ctx.Invaders.HasTown)
				await ctx.GameState.MoveInvader( ctx.Invaders[InvaderSpecific.Town]>0? InvaderSpecific.Town:InvaderSpecific.Town1, ctx.Target, drowningOcean );
			await ctx.InvadersOn.Destroy( Invader.Town, 1 );

			await ctx.GameState.DestroyDahan(ctx.Target,1,Cause.Power);
		}
	}
}
