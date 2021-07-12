using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class CallToIsolation {


		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		static public async Task Act(ActionEngine engine){
			var (spirit,gameState) = engine;

			// range 1, dahan
			var target = await engine.TargetSpace_Presence(1,gameState.HasDahan);

			var grp = gameState.InvadersOn(target);
			int pushCount = gameState.GetDahanOnSpace(target);
			const string pushDahanKey = "push 1 dahan";
			bool pushDahan = !grp.HasExplorer && !grp.HasTown
				|| await engine.SelectText("Select option",pushDahanKey,$"push {pushCount} explorer or towns") == pushDahanKey;

			if( pushDahan )
				await engine.Push1Dahan(target);
			else {

				// push 1 explorer/town per dahan
				Invader[] CalcAvailableInvaders() => grp.InvaderTypesPresent.Where(i=>i.Summary.IsIn("E@1","T@2","T@1")).ToArray();
				Invader[] invadersToPush = CalcAvailableInvaders();
				while(0<pushCount && 0<invadersToPush.Length){
					var invader = await engine.SelectInvader("Invader to push",invadersToPush,true);
					if(invader==null) break;
					await engine.PushInvader(target,invader);

					invadersToPush = CalcAvailableInvaders();
					--pushCount;
				}
				gameState.UpdateFromGroup(grp);
			}

		}

	}
}
