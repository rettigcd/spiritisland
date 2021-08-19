using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PowerStorm {

		[MajorCard("Powerstorm",3,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public Task ActionAsync( IMakeGamestateDecisions _,Spirit target){
			
			// target spirit gains 3 energy
			target.Energy += 3;

			// once this turn, target may repeat a power card by paying its cost again
			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost
			int repeats = target.Elements.Contains("2 sun,2 fire,3 air") ? 3 : 1;

			while(repeats-->0)
				target.AddActionFactory( new RepeatCardForCost( ) );
			return Task.CompletedTask;
		}

		public class RepeatCardForCost : IActionFactory {
			public Speed Speed => Speed.FastOrSlow;
			public string Name => "Replay Cards for Cost";
			public IActionFactory Original => this;
			public string Text => Name;

			public async Task Activate( Spirit self, GameState gameState ) {
				var cards = self.DiscardPile.Where(c=>c.Cost<=self.Energy).ToArray();
				var card = (PowerCard)await self.SelectFactory("Select card to replay",cards,true);
				if( card == null ) return;
				self.Energy -= card.Cost;
				await card.Activate(self,gameState);
			}
		}

	}
}
