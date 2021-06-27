using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[PowerCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : TargetSpiritAction{

		public const string Name = "Boon of Vigor";

		public BoonOfVigor(Spirit self,GameState gameState):base(self,gameState){}

		protected override void SelectSpirit(Spirit spirit){
			spirit.Energy += (spirit==self) ? 1 : spirit.PurchasedCards.Count;
		}

	}

}