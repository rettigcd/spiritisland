
using SpiritIsland.Core;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Basegame {

	/*

	=========================================================
	A Spread of Rampant Green

	* +1 presense to jungle or wetland (Always do this + one of the following)
	* reclaim, +1 power card
	* +1 presense range 1, play +1 extra card this turn
	* +1 power card, +3 energy

	0 1 plant 2 2 plant 3
	1 1 2 2 3 4

	Innate-1:  Creepers Tear Into Mortar  => slow, 0,any
	1 moon 2 plant   1 damange to 1 town or city
	2 moon 3 plant   repeat this power
	3 moon 4 plant   repeat this power again

	Innate-2:   All-Enveloping Green => fast, within 1 of sacred, any
	1 water 3 plant  defend 2
	2 water 4 plant  instead, defend 4
	3 water 1 rock 5 plant  also, remove 1 blight

	Special Ruels - Choke the land with green - whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land
	Special Rules - Stedy regeneration - when adding presense to the board via growth, yo may optionally use your destroyed presense.  If the island is helathy, do so freely.  If the island is blighted, doing so costs 1 energy per destroyed presense you add.
	Setup: 1 in the highest numbered wedland and 1 in the jungle without any dahan

Green
	Infinite vitality
	The trees and stones speak of war
	---
	drift down into slumber
	gift of living energy
	lure of the unknown
	enticing spender

*/

	public class RampantGreen : Spirit {

		public const string Name = "A Spread of Rampant Green";

		public override string Text => Name;

		public RampantGreen():base(
			new Track[] { Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 },
			new Track[] { Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 },
			new NullPowerCard( "A", 0, Speed.Fast ),
			new NullPowerCard( "B", 0, Speed.Fast ),
			new NullPowerCard( "C", 0, Speed.Fast ),
			new NullPowerCard( "D", 0, Speed.Fast )
		) {
			var placeOnWetlandOrJungle = new PlacePresence(2, Filter.JungleOrWetland, "W / J");

			GrowthOptions = new GrowthOption[]{
				// reclaim, +1 power card
				new GrowthOption(
					placeOnWetlandOrJungle,
					new ReclaimAll(), 
					new DrawPowerCard(1)
				),
				// +1 presense range 1, play +1 extra card this turn
				new GrowthOption(
					placeOnWetlandOrJungle,
					new PlacePresence(1),
					new PlayExtraCardThisTurn()
				),
				// +1 power card, +3 energy
				new GrowthOption(
					placeOnWetlandOrJungle,
					new GainEnergy(3), 
					new DrawPowerCard()
				),
			};

		}

		public override int NumberOfCardsPlayablePerTurn => base.NumberOfCardsPlayablePerTurn + tempCardBoost;

		public override void PurchaseAvailableCards( params PowerCard[] cards ) {
			base.PurchaseAvailableCards( cards );
			tempCardBoost = 0;
		}
		public int tempCardBoost = 0;

		public override void Initialize( Board _, GameState _1 ) {
			throw new System.NotImplementedException();
		}


	}

}
