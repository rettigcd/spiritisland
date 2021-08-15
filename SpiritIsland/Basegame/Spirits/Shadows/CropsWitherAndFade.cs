using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class CropsWitherAndFade {

		[SpiritCard("Crops Wither and Fade",1,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0)]
		static public async Task Act(ActionEngine engine,Space target){
			var (_,gs) = engine;

			// 2 fear
			gs.AddFear(2);

			// replace 1 town with 1 explorer
			// OR
			// replace 1 city with 1 town
			var invaders = gs.InvadersOn(target).FilterBy(Invader.City,Invader.Town);
			if(invaders.Length==0) return;
			InvaderSpecific invader = await engine.SelectInvader("Select invader to down-grade",invaders);
			InvaderSpecific newInvader = (invader.Generic == Invader.Town) ? InvaderSpecific.Explorer
				: invader.Health==1 ? InvaderSpecific.Town1
				: InvaderSpecific.Town; // also replaces C@2 with T@2
			gs.Adjust(target,newInvader,1);
			gs.Adjust(target,invader,-1);
		}

	}

}
