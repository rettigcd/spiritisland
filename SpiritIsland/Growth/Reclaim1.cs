using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Reclaim1 : GrowthAction {

		public PowerCard Card {get; set;}

		public override void Apply() {
			if( Card == null )
				throw new InvalidOperationException("reclaim 1: no card specified");
			if( spirit.DiscardPile.Contains(Card) ){
				spirit.DiscardPile.Remove(Card);
				spirit.Hand.Add(Card);
			}
			Card = null; // ensure it must be set each time.
		}

		public override bool IsResolved => Card != null;

		public override IOption[] Options => throw new NotImplementedException();

		static public IResolver Resolve(PowerCard card) => new Resolver(card);

		class Resolver : IResolver {

			readonly PowerCard card;

			public Resolver(PowerCard card){
				this.card = card;
			}

			public void Apply(List<IAction> growthActions){
				Reclaim1 action = growthActions.OfType<Reclaim1>()
					.Where(x=>x.Card == null) // not yet specified - ensures Resolves don't out number actual actions
					.FirstOrDefault();
				if(action == null) throw new Exception("Reclaim action not found.");
				action.Card = card;
				action.Apply();
				action.Resolved(action.spirit);
			}

		}


	}

}
