using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class CallToIsolation {


		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		static public async Task Act(ActionEngine engine){
			var (spirit,gameState) = engine;

			// range 1, dahan
			var target = await engine.SelectSpace("Select target", spirit.Presence.Range(1).Where(gameState.HasDahan) );

			var grp = gameState.InvadersOn(target);
			int pushCount = gameState.GetDahanOnSpace(target);
			const string pushDahanKey = "push 1 dahan";
			bool pushDahan = !grp.HasExplorer && !grp.HasTown
				|| await engine.SelectText("Select option",pushDahanKey,$"push {pushCount} explorer or towns") == pushDahanKey;

			if( pushDahan ){

				// push 1 dahan
				var destination = await engine.SelectSpace("Push dahan to",target.Neighbors.Where(x=>x.IsLand));
				gameState.AddDahan(target,-1);
				gameState.AddDahan(destination,1);

			} else {

				// push 1 explorer/town per dahan
				var invadersToPush = grp.InvaderTypesPresent.Where(i=>i.Summary.IsIn("E@1","T@2","T@1")).ToArray();
				while(invadersToPush.Length>0 && pushCount>0){
					var invader = await engine.SelectInvader("Invader to push",invadersToPush,true);
					if(invader==null) break;
					var destination = await engine.SelectSpace("Push dahan to",target.Neighbors.Where(x=>x.IsLand));
					--grp[invader]; // source
					gameState.Adjust(destination,invader,1); // destination
				}
				gameState.UpdateFromGroup(grp);
			}

		}

	}
}
