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
			var grp = gs.Tokens[ target ];
			var invaders = grp.OfAnyType(Invader.City,Invader.Town);
			if(invaders.Length==0) return;
			var invader = await ctx.Self.Action.Choose( new SelectInvaderToDowngrade( ctx.Target, invaders, Present.IfMoreThan1 ) );

			Token newInvader = (invader.Generic == Invader.Town) ? Invader.Explorer[1]
				: invader.Health==1 ? Invader.Town[1]
				: Invader.Town[2]; // also replaces C@2 with T@2
			grp.Adjust(newInvader,1);
			grp.Adjust(invader,-1);
		}

	}

}
