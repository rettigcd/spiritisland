using System;
using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests {

	public class FearImage_Tests {

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public void DisplayFearCardConstructor(int terrorLevel) {
			List<IFearOptions> fearCards = new List<IFearOptions>();
			fearCards.AddRange( SpiritIsland.Basegame.FearCards.GetFearCards() );
			fearCards.AddRange( SpiritIsland.BranchAndClaw.FearCards.GetFearCards() );

			foreach(var c in fearCards) {
				// string fearCard = c.GetType().Name; // capture which card it is before the exception
				_ = new ActivatedFearCard(c,terrorLevel);
			}

		}


	}

}
