using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CropsWitherAndFade {

		[SpiritCard("Crops Wither and Fade",1,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0)]
		static public async Task ActAsync( TargetSpaceCtx ctx ){
			var target = ctx.Target;
			var (_,gs) = ctx;

			// 2 fear
			ctx.AddFear(2);

			// replace 1 town with 1 explorer
			// OR
			// replace 1 city with 1 town
			var invaders = gs.InvadersOn(target).FilterBy(Invader.City,Invader.Town);
			if(invaders.Length==0) return;
			var invader = await ctx.Self.Action.Choose( new SelectInvaderToDowngrade( ctx.Target, invaders, Present.IfMoreThan1 ) );

			InvaderSpecific newInvader = (invader.Generic == Invader.Town) ? InvaderSpecific.Explorer
				: invader.Health==1 ? InvaderSpecific.Town1
				: InvaderSpecific.Town; // also replaces C@2 with T@2
			gs.Adjust(target,newInvader,1);
			gs.Adjust(target,invader,-1);
		}

	}

}
