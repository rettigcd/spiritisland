using System;

namespace SpiritIsland.PowerCards {

	[PowerCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : IAction{

		public const string Name = "Boon of Vigor";

		public BoonOfVigor(Spirit self,GameState gameState){
			this.self = self;
			this.gameState = gameState;
		}

		public bool IsResolved => target != null;

		public void Apply() {
			this.target.Energy += (target==self) ? 1 : target.PurchasedCards.Count;
		}

		public IOption[] Options { get{
				if( target == null )
					return gameState.Spirits;
				return new IOption[0]; // fully resolved, is this the best we can do ???
		}}

		public void Select(IOption option) {
			if( target == null ){
				target = (Spirit)option;
				return;
			}

			throw new InvalidOperationException(); // ???
		}

		readonly Spirit self;
		readonly GameState gameState;
		Spirit target;

	}

}