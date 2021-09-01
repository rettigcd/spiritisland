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
			if( ctx.PowerInvaders.Counts.Has(Invader.Explorer))
				await ctx.GameState.Invaders.Move(Invader.Explorer[1],ctx.Target,drowningOcean);

			// drop town in the ocean to drown
			if(ctx.PowerInvaders.Counts.Has(Invader.Town))
				await ctx.GameState.Invaders.Move( ctx.PowerInvaders.Counts[Invader.Town[2]]>0 ? Invader.Town[2] : Invader.Town[1], ctx.Target, drowningOcean );
			await ctx.PowerInvaders.Destroy( Invader.Town, 1 );

			await ctx.GameState.Dahan.Destroy(ctx.Target,1,Cause.Power);
		}
	}
}
