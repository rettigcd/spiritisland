using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	/*
	================================================================
	Bringer of Dreams and Nightmares
	* reclaim, +1 power card
	* reclaim 1, add presense range 0
	* +1 power card, +1 pressence range 1
	* add presense range Dahan or Invadors, +2 energy

	2 air 3 moon 4 any 5
	2 2 2 3 3 any

	Innate 1 - Spirits May Yet Dream => fast any spirit
	2 moon 2 air  Turn any face-down fear card face-up
	3 moon   Target spirit gains an element they have at least 1 of

	Innate 2 - Night Terrors => fast range 0, invaders
	1 moon 1 air    1 fear
	2 moon 1 air 1 beast   +1 fear
	3 moon 2 air 1 beast   +1 fear
	Special rules - To Dream a Thousand Deaths - Your power never cause Damange, nor can they destory anything other than your own presense 
	When a power of oyours would destroy exporer/town/city generate 0/2/5 fear instead.  The power pushes explorers / towns it would destroy
	Setup:  2 presense in highest numbered sands

	Dread Apparations => 2 => fast, range 1, invaders => moon, air => When powers generate fear in target land, defend 1 per fear.  1 fear  (fear from to Dream a Thousands Deaths counts.  Fear from destroying town/cities does not.)
	Call on Midnight's Dream => 0 => fast, range 0, any => moon, animal => if target land has dahan, gain a major power.  If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost -OR- if invaders are present, 2 fear
	Dreams of the Dahan => 0 => fast, range 2, any => moon, air => gather up to 2 dahan -OR- if target land has town/city, 1 fear for each dahan, to a maximum of 3 fear
	Predatory Nightmares => 2 => slow, 1 from sacred site, invaders => moon, fire, mountain, animal => 2 damange.  Push up to 2 dahan.  When your Powers would destroy invaders, instead they ggenerate fear and/or push those invaders

	*/

	public class Bringer : Spirit {

		public override string Text => "Bringer of Dreams and Nightmares";

		public Bringer(){
			static bool onDahanOrInvadors(Space bs,GameState gameState) { // !!! hook in here! instead of Criteria
				return gameState.HasDahan(bs) || gameState.HasInvaders(bs);
			}

			GrowthOptions = new GrowthOption[]{
				// reclaim, +1 power card
				new GrowthOption(new ReclaimAll(),new DrawPowerCard(1)),
				// reclaim 1, add presence range 0
				new GrowthOption(new Reclaim1(), new PlacePresence(0) ),
				// +1 power card, +1 pressence range 1
				new GrowthOption(new DrawPowerCard(1), new PlacePresence(1) ),
				// add presense range Dahan or Invadors, +2 energy
				new GrowthOption(new GainEnergy(2), new PlacePresence(4,onDahanOrInvadors,"dahan or invaders"))
			};

		}

		//	2 air 3 moon 4 any 5
		protected override int[] EnergySequence => new int[]{2,2,3,3,4,4,5};

		// card:	2 2 2 3 3 any
		protected override int[] CardSequence => new int[]{ 2,2,2,3,3,3 };

		public override int Elements( Element el ) {
			return new Element[]{ 
				Element.None, 
				Element.Air, 
				Element.None, 
				Element.Moon,
				Element.None,
				Element.Any,
				Element.None
			}	.Take(RevealedEnergySpaces)
				.Count(x=>x==el)
			+ new Element[]{ 
				Element.None, 
				Element.None, 
				Element.None, 
				Element.None, 
				Element.None,
				Element.Any
			}	.Take(RevealedCardSpaces)
				.Count(x=>x==el);
		}

		public override void InitializePresence( Board _ ) {
			throw new System.NotImplementedException();
		}


	}

}
