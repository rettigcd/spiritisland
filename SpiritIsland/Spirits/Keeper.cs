using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiritIsland {

	/*


	==========================================
	Keeper of the Forbidden Wilds
	(pick 2)
	* reclaim, +1 energy
	* +1 power card
	* add presense range 3 containing wilds or presense, +1 energy
	* -3 energy, +1 power card, add presense to land without blight range 3

	2 sun 4 5 plant 7 8 9
	1 2 2 3 4 5

	Innate - Punish Those Who Trespass => slow, range 0, any
	2 sun 1 fire 2 plant    2 damange.  destroy 1 dahan
	2 sun 2 fire 3 plant   +1 damange per sun-plant you have
	4 plant    you may split this powers damange however desired between target land and one of your lands

	Innate - Spreading Wilds  => slow, range 1, no blight
	2 sun    push 1 explorer from target land per 2 sun
	1 plant   if target land has no explorers, add 1 hairy snail
	3 plant  this power has +1 range
	1 air    this power has +1 range

	Special Rules - Forbidden Ground - any time you create a sacred site, push all dahan from that land.  Dahan events never move dahan to you sacred site but powers can do so.
	Set Up - put 1 presense and 1 hairy snail on your starting board in the highest numbered jungle

	Boon of Growing Power => 1 => slow, any spirit => sun, moon, plant => target spirit gains a power card.  If you target another spirit, they also gain 1 energy
	Towering Wrath => 3 => slow, withing 1 of S.S., any => sun, fire, plant => 2 fear.  For each your SS in or adjacent to target land, 2 damage.  Destroy all Dahan
	Regrow from Roots => 1 => slow, range 1, jungle or wetlands => water, rock, plant => if there are 2 blight or fewer in target land, remove 1 blight
	Sacrosanct Wilderness => 2 => fast, range 1, no blight => sun, rock, plant => push 2 dahan.  (2 damange per hairy snail in target land. -OR- add 1 hairy snail)

	 */

	public class Keeper : Spirit {
		public override GrowthOption[] GetGrowthOptions( GameState gameState ) {

			bool presenceOrWilds(Space s) => this.Presence.Contains(s) || gameState.HasWilds(s);
			bool noBlight(Space s) => s.Terrain != Terrain.Ocean && !gameState.HasBlight(s);

			return new GrowthOption[]{
				new GrowthOption(
					new ReclaimAll()	    // A
					,new GainEnergy(1)     // A
					,new DrawPowerCard(1)	// B
				)
				,new GrowthOption(
					new ReclaimAll()        // A
					,new GainEnergy(2)     // A & C
					,new PlacePresence(3,presenceOrWilds) // C
				)
				,new GrowthOption(
					new ReclaimAll()          // A
					,new GainEnergy(1-3)     // A & D
					,new DrawPowerCard(1)	  // D
					,new PlacePresence(3,noBlight) // D
				)
				,new GrowthOption(
					new DrawPowerCard(1)	// B
					,new GainEnergy(1)     // C
					,new PlacePresence(3,presenceOrWilds) // C
				)
				,new GrowthOption(
					new DrawPowerCard(1)	// B
					,new GainEnergy(-3)     // D
					,new DrawPowerCard(1)	  // D
					,new PlacePresence(3,noBlight) // D
				)
				,new GrowthOption(
					new GainEnergy(1)     // C
					,new PlacePresence(3,presenceOrWilds) // C
					,new PlacePresence(3,noBlight) // D
					,new GainEnergy(-3)     // D
					,new DrawPowerCard(1)	  // D
				)
			};
		}

		// energy:	2 sun 4 5 plant 7 8 9

		protected override int[] EnergySequence => new int[]{ 2,2,4,5,5,7,8,9};

		protected override int[] CardSequence => new int[]{ 1, 2, 2, 3, 4, 5 };

		public override int Elements( Element el ) {
			return new Element[]{
				Element.None,
				Element.Sun,
				Element.None,
				Element.None,
				Element.Plant,
				Element.None,
				Element.None,
				Element.None
			}.Take( RevealedEnergySpaces )
				.Count( x => x == el );
		}


	}

}
