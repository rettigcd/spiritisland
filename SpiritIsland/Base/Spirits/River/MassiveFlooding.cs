using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	[PowerLevel(0,Element.Sun,Element.Water,Element.Water)]
	public class MassiveFlooding : BaseAction {

		public const string Name = "Massive Flooding";
		public const string k1 = "Push 1 E/T";
		public const string k2 = "2 damage, Push up to 3 explorers and/or towns";
		public const string k3 = "2 damage to all";

		Space space;

		public MassiveFlooding(Spirit spirit,GameState gameState):base(spirit,gameState){
			_ = ActionAsync(spirit);
		}

		async Task ActionAsync(Spirit spirit){
			var elements = spirit.AllElements;

			int count = new int[]{
				elements[Element.Sun],
				elements[Element.Water]-1,
				elements[Element.Earth]==0?2:3
			}.Min();
			if(count == 0) return;
			
			string key = await engine.SelectText("Select Innate option", new string[]{ k1,k2,k3}.Take(count).ToArray() );

			space = await engine.Api.TargetSpace_SacredSite(1,HasExplorersOrTowns);

			switch(key){
				case k1: await Option1(); break;
				case k2: await Option2(); break;
				case k3: Option3(); break;
			}

		}

		bool HasExplorersOrTowns(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

		async Task Option1(){
			await PushTownOrExplorer(1,false);
		}

		async Task Option2(){
			// * 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns
			gameState.DamageInvaders(space,2);
			await PushTownOrExplorer(3,true);
		}

		void Option3(){
			//` * 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader
			var group = gameState.InvadersOn(space);
			var invaderTypes = group.InvaderTypesPresent.ToArray(); // copy so we can modify
			foreach(var invader in invaderTypes){
				// add the damaged invaders
				group[ invader.Damage(2) ] += group[invader];
				// clear the healthy invaders
			}
			gameState.UpdateFromGroup(group);
		}

		async Task PushTownOrExplorer(int count, bool allowShortCircuit) {
			var invadersToPush = InvadersForPushing();
			while(0<count && 0<invadersToPush.Length) {
				var invader = await engine.SelectInvader( "Select invader to push.", invadersToPush, allowShortCircuit );
				if(invader == null) break;
				await engine.PushInvader(space,invader);

				invadersToPush = InvadersForPushing();
				--count;
			}
		}

		Invader[] InvadersForPushing() => gameState.InvadersOn( space )
			.FilterBy( Invader.Town, Invader.Explorer );
	}

}
