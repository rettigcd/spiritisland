using System;

namespace SpiritIsland.PowerCards {

	[PowerCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : BaseAction{

		public const string Name = "Boon of Vigor";

		public BoonOfVigor(Spirit self,GameState gameState):base(gameState){
			this.self = self;

			engine.decisions.Push(new TargetSpirit(gameState.Spirits,SelectSpirit));

		}

		void SelectSpirit(Spirit spirit){
			spirit.Energy += (spirit==self) ? 1 : spirit.PurchasedCards.Count;
		}

		readonly Spirit self;

	}

}