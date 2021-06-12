using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Reclaim1 : GrowthAction {

		public Reclaim1(Spirit spirit):base(spirit){}

		public PowerCard Card {get; set;}

		public override void Apply() {
			if( Card == null )
				throw new InvalidOperationException("reclaim 1: no card specified");
			if( spirit.PlayedCards.Contains(Card) ){
				spirit.PlayedCards.Remove(Card);
				spirit.AvailableCards.Add(Card);
			}
			Card = null; // ensure it must be set each time.
			spirit.MarkResolved( this );
		}

		public override bool IsResolved => Card != null;

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
			}

		}


	}

}
