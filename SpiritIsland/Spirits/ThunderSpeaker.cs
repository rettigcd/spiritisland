using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	/*
	=== Thunder Speaker

	* reclaim, +1 power card, +1 power card
	* +1 presense with 2 - contining dahan, +1 presense within 1 - containing dahan
	* +1 presense within 1, +4 energy

	1 air 2 fire sun 3
	1 2 2 3 reclaim-1 3 4

	Innate-1:  Gather the Warriors => slow, range 1, any
	4 air   this power may be fast
	1 beast    gather 1 dahan per air, push 1 dahan per sun
	Innate-2:  Lead the Furious Assult
	4 air   this power may be fast
	2 sun 1 fire   Destroy 1 town for every 2 dahan in target land
	4 sun 3 fire   Destory 1 city for every 3 dahan in target land
	Special Rules - Ally of the Dahan - Your presense may move with dahan
	Special Rules - Swort to Victory - For each dahan stroyed by invaters ravaging a land, destroy 1 of your presense withing 1
	Setup:  put 1 presense in each of the 2 lands with the most presense

	Sudden Ambush => 2 => fast, range 1, any => fire, air, animal => you may gather 1 dahan. Each dahan destroys 1 explorer
	Words of Warning => 1 => fast, range 1, has dahan => defend 3.  During ravage, dahan in target land deal damange simultaneously with invaders
	Manifestation of Power and Glory => 3 => slow, range 0, has dahan => sun, fire, air => 1 fear.  each dahan deals damange equal to the number of your presense in the target land
	Voice of thunder => 0 => slow, range 1, any => sun, air => push up to 4 dahan -OR- If invaders are present, 2 fear
	*/

	public class ThunderSpeaker : Spirit {
		
		public override GrowthOption[] GetGrowthOptions(){

			static bool onDahan(Space bs,GameState gameState) => gameState.HasDahan(bs);
			var opt1Actions = new List<GrowthAction>{
				new PlacePresence(2,onDahan ),
				new PlacePresence(1,onDahan )
			};

			var opt2Actions = new List<GrowthAction>{
				new PlacePresence(1), new GainEnergy(4)
			};

			if( RevealedCardSpaces >= 5){
				opt1Actions.Add( new Reclaim1() );
				opt2Actions.Add( new Reclaim1() );
			}

			return new GrowthOption[]{
				new GrowthOption( new ReclaimAll(), new DrawPowerCard(2) ),
				new GrowthOption( opt1Actions ),
				new GrowthOption( opt2Actions )
			};
		}

		protected override int[] EnergySequence => new int[]{1, 1, 2, 2, 2, 3 };
		protected override int[] CardSequence => new int[]{1, 2, 2, 3, 3, 3, 4 };

		public override int Elements( Element el ) {
			return new Element[]{ 
				Element.None, 
				Element.Air, 
				Element.None, 
				Element.Fire,
				Element.Sun,
				Element.None
			}	.Take(RevealedEnergySpaces)
				.Count(x=>x==el);
		}

	}

}
