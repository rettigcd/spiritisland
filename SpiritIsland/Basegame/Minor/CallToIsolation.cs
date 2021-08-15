using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class CallToIsolation {

		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task Act(ActionEngine engine,Space target){
			var (spirit,gameState) = engine;

			var grp = gameState.InvadersOn(target);
			int pushCount = gameState.GetDahanOnSpace(target);
			const string pushDahanKey = "push 1 dahan";
			bool pushDahan = !grp.HasExplorer && !grp.HasTown
				|| await engine.SelectText("Select option",pushDahanKey,$"push {pushCount} explorer or towns") == pushDahanKey;

			if( pushDahan )
				await engine.PushUpToNDahan(target,1);
			else
				// push 1 explorer/town per dahan
				await engine.PushUpToNInvaders(target,pushCount,Invader.Town,Invader.Explorer);

		}

	}
}
