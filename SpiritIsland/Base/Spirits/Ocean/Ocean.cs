using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	/*
	====================================
	Oceans Hungry Grasp
	* reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
	* +1 presence range any ocean, +1 presense in any ociean, +1 energy
	* gain power card, push 1 presense from each ocian,  add presense on costal land range 1

	0 moon water 1 earth wter 2
	1 2 2 3 4 5

	Innate - Pound Ships to Splinters => fast, range 0, costal
	1 moon 1 air 2 water    1 fear
	2 moon 1 air 3 water   +1 fear
	3 moon 2 air 4 water   +2 fear

	Innate - Ocean Breaks the Shore  => slow range 0, costal
	2 water 1 mountain   drown 1 town
	3 water 2 mountain   instead drown 1 city
	4 water3 mountain     also, drown 1 town or city

	Special - Ocean in Play - You may add/move presnece into  oceans but may not add/move presense into inland lands.  On
	On boards with you presense, treat ocieans as coastal wetalnads for powers and blight.  YOu drown any invaters or dahan moved into those ocians
	Drowning - destorys drawned pieces, placing draowned invaters here.  At any time you may exchange X health of these invatores for 1 energy where x = number of players
	Setup - 1 in ocean and 1 in costal land of your choice.

	Tidal Boon => 1 => slow, any spirit => moon, water, rock => target spirit gains 2 energy and may push 1 town and up to 2 dahan from one of their lands.   If dahan are pushed to your ocean, you may move them to any costal land instead of drowning them.
	Call of the Deeps => 0 => fast, range 0, costal => moon, air, water => Gather 1 explorer, if target land is the ocean, you may gather another explorer
	Grasping Tide => 1 => fast, range 1, cotal => moon, water => 2 fear, defend 4
	Swallow the Land-Dwellers => 0 => slow, range 0, costal => water, rock => drown 1 explorer, 1 town, and 1 dahan


	 */

	public class Ocean : Spirit {

		public Ocean():base(
			new NullPowerCard( "A", 0, Speed.Fast ),
			new NullPowerCard( "B", 0, Speed.Fast ),
			new NullPowerCard( "C", 0, Speed.Fast ),
			new NullPowerCard( "D", 0, Speed.Fast )
		) {

			// !!! add test that oceans containing 2 presence only push 1 of them out.
			// Option 1 - reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			// Option 2 - +1 presence range any ocean, +1 presense in any ociean, +1 energy
			// Option 3 - gain power card, push 1 presense from each ocean,  add presense on costal land range 1
			GrowthOptions = new GrowthOption[]{ 
				new GrowthOption(
					new GatherPresenceIntoOcean(),
					new ReclaimAll(),
					new DrawPowerCard(),
					new GainEnergy(2)
				), 
				new GrowthOption(
					new GainEnergy(1),
					new PlaceInOcean(),
					new PlaceInOcean()
				), 
				new GrowthOption( 
					new PushPresenceFromOcean(),
					new DrawPowerCard(),
					new PlacePresence(1,(s,_)=>s.IsCostal, "coatal" )
				)
			};

		}

		public override string Text => "Ocean's Hungry Grasp";

		// energy: 0 moon water 1 earth water 2
		protected override int[] EnergySequence => new int[]{ 0,0,0,1,1,1,2 } ;

		protected override int[] CardSequence => new int[]{ 1, 2, 2, 3, 4, 5, };

		public override int Elements( Element el ) {
			return new Element[]{
				Element.None,
				Element.Moon,
				Element.Water,
				Element.None,
				Element.Earth,
				Element.Water,
				Element.None
			}.Take( RevealedEnergySpaces )
				.Count( x => x == el );
		}

		public override void Initialize( Board _, GameState _1 ) {
			throw new System.NotImplementedException();
		}

	}

}
