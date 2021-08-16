using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class CallToIsolation {

		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task Act(ActionEngine engine,Space target){
			var (_,gameState) = engine;

			var grp = gameState.InvadersOn(target);
			int pushCount = gameState.GetDahanOnSpace(target); // push 1 explorer/town per dahan

			bool pushDahan = !grp.HasExplorer && !grp.HasTown
				|| await engine.SelectFirstText("Select option", "push 1 dahan", $"push {pushCount} explorer or towns");

			if( pushDahan )
				await engine.PushUpToNDahan(target,1);
			else
				// push 1 explorer/town per dahan
				await engine.PushUpToNInvaders(target,pushCount,Invader.Town,Invader.Explorer);

		}

	}
}
