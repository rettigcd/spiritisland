using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class RepeatCardForCost : IActionFactory {

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Fast || speed == Speed.Slow;
		public bool IsInactiveAfter( Speed speed ) => speed == Speed.Slow;

		public string Name => "Replay Cards for Cost";
		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState gameState ) {
			var cards = self.DiscardPile.Where( c => c.Cost <= self.Energy ).ToArray();
			var card = await self.SelectPowerCard( "Select card to replay", cards, CardUse.Replay, Present.Done );
			if(card == null) return;
			self.Energy -= card.Cost;
			await card.ActivateAsync( self, gameState );
		}

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect
	}

}
