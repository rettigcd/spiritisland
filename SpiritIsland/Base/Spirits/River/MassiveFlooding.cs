using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class MassiveFlooding {

		public const string Name = "Massive Flooding";
		public const string k1 = "Push 1 E/T";
		public const string k2 = "2 damage, Push up to 3 explorers and/or towns";
		public const string k3 = "2 damage to all";

		[InnatePower(MassiveFlooding.Name,Speed.Slow)]
		[PowerLevel(0,Element.Sun,Element.Water,Element.Water)]
		[FromSacredSite(1,Filter.TownOrExplorer)]
		static public async Task ActionAsync(ActionEngine engine,Space target){
			var (spirit,gameState) = engine;
			var elements = spirit.AllElements;

			int count = new int[]{
				elements[Element.Sun],
				elements[Element.Water]-1,
				elements[Element.Earth]==0?2:3
			}.Min();
			if(count == 0) return;
			
			string key = await engine.SelectText("Select Innate option", new string[]{ k1,k2,k3}.Take(count).ToArray() );
			switch(key){
				case k1: 
					await engine.PushUpToNInvaders(target,1,Invader.Town,Invader.Explorer); 
					break;
				case k2: 
					gameState.DamageInvaders(target,2);
					await engine.PushUpToNInvaders(target,3,Invader.Town,Invader.Explorer);
					break;
				case k3: 
					var group = gameState.InvadersOn(target);
					var invaderTypes = group.InvaderTypesPresent.ToArray(); // copy so we can modify
					foreach(var invader in invaderTypes){
						// add the damaged invaders
						group[ invader.Damage(2) ] += group[invader];
						// clear the healthy invaders
					}
					gameState.UpdateFromGroup(group);
					break;
			}

		}

	}

}
