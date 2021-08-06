using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

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

Thunderspeaker
	Wrap in Wings of Sunlight
	Talons of Lightning
	or maybe...
	The Trees and Stones speak of war
	Vigor of the breaking down
	----
	elemental boon
	drought
	reaching grasp
	veil the nights hunt



	*/

	public class ThunderSpeaker : Spirit {

		public override string Text => "Thunder Speaker";

		public ThunderSpeaker():base(
			new NullPowerCard( "A", 0, Speed.Fast ),
			new NullPowerCard( "B", 0, Speed.Fast ),
			new NullPowerCard( "C", 0, Speed.Fast ),
			new NullPowerCard( "D", 0, Speed.Fast )
		){
			static bool onDahan(Space bs,GameState gameState) => gameState.HasDahan(bs);

			GrowthOptions = new GrowthOption[]{
				new GrowthOption( 
					new ReclaimAll(), 
					new DrawPowerCard(2)
				),
				new GrowthOption( 
					new PlacePresence(2,onDahan,"dahan"),
					new PlacePresence(1,onDahan,"dahan")
				),
				new GrowthOption( 
					new PlacePresence(1), 
					new GainEnergy(4)
				)
			};

		}

		public override void Grow( GameState gameState, int optionIndex ) {
			GrowthOption option = this.GetGrowthOptions()[optionIndex];
			foreach (var action in option.GrowthActions)
				AddActionFactory(action);

			if( RevealedCardSpaces >= 5 && optionIndex>0)
				AddActionFactory(new Reclaim1());

			RemoveResolvedActions(gameState,Speed.Growth);
		}

		protected override int[] EnergySequence => new int[]{1, 1, 2, 2, 2, 3 };
		protected override int[] CardSequence => new int[]{1, 2, 2, 3, 3, 3, 4 };

		protected override IEnumerable<Element> TrackElements() {
			return new Element[]{ 
				Element.None, 
				Element.Air, 
				Element.None, 
				Element.Fire,
				Element.Sun,
				Element.None
			}	.Take(RevealedEnergySpaces)
				.Where(el=>el!=Element.None);
		}

		public override void Initialize( Board _, GameState _1 ) {
			throw new System.NotImplementedException();
		}

	}

}
