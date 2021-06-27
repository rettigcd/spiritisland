using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	// !!Split
	public class Reclaim1 : GrowthAction {

		public PowerCard Card {get; set;}

		public override IOption[] Options => spirit.DiscardPile.Cast<IOption>().ToArray();

		public override void Select( IOption option ) {
			Card = (PowerCard)option;
		}

		public override void Apply() {
			if( Card == null ) throw new InvalidOperationException("reclaim 1: no card specified");
			if( spirit.DiscardPile.Contains(Card) ){
				spirit.DiscardPile.Remove(Card);
				spirit.Hand.Add(Card);
			}
			Card = null; // ensure it must be set each time.
		}

		public override bool IsResolved => Card != null;

	}

}
