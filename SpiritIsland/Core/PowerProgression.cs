using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Core {

	// Plug into the DrawPowerCard API on the spirit
	class PowerProgression {
		readonly List<PowerCard> cards;
		public PowerProgression( params PowerCard[] cards ) {
			this.cards = cards.ToList();
		}
		public Task DrawCard( ActionEngine eng, string typeString) {
			var (spirit, _) = eng;

			// typeString should be "major", "minor", or "" if don't care
			var newCard = typeString switch{
				"major" => cards.First(c=>c.PowerType==PowerType.Major),
				"minor" => cards.First(c=>c.PowerType==PowerType.Minor),
				"" => cards[0],
				_ => throw new ArgumentException(typeString+" is not valid. should be major, minor or empty string")
			};
			cards.Remove( newCard );

			spirit.RegisterNewCard( newCard );
			if(newCard.PowerType == PowerType.Major)
				spirit.AddActionFactory( new ForgetPowerCard() ); // !!! do this right now, don't make it another factory
			return Task.CompletedTask;
		}

	}


}
