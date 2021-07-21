using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	[FromSacredSite(1,Filter.TownOrExplorer)]
	public class MassiveFlooding {

		public const string Name = "Massive Flooding";

		[InnateOption(Element.Sun,Element.Water,Element.Water)]
		static public Task Option1Async(ActionEngine engine,Space target){
			// Push 1 Town/Explorer
			return engine.PushUpToNInvaders(target,1,Invader.Town,Invader.Explorer); 
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water)]
		static public async Task Option2Async(ActionEngine engine,Space target){
			engine.GameState.DamageInvaders(target,2);
			await engine.PushUpToNInvaders(target,3,Invader.Town,Invader.Explorer);
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water,Element.Water,Element.Earth)]
		static public Task Option3Async(ActionEngine engine,Space target){
			var group = engine.GameState.InvadersOn(target);
			var invaderTypes = group.InvaderTypesPresent.ToArray(); // copy so we can modify
			foreach(var invader in invaderTypes){
				// add the damaged invaders
				group[ invader.Damage(2) ] += group[invader];
				// clear the healthy invaders
			}
			engine.GameState.UpdateFromGroup(group);
			return Task.CompletedTask;
		}

	}

}
