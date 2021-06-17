﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiritIsland {

	/*


	================================================
	Sharp Fangs Behind the Leaves
	(pick 2)

	 * cost -1, reclaim cards, gain +1 power card
	 * add a presense to jungle or a land with beasts (range 3)
	 * gain power card, gain +1 energy
	 * +3 energy

	1 animal plant 2 animal 3 4
	2 2 3 relaim-1 4 5&reclaim-1

	Innate - Ranging Hunt  => fast, range 1, no blight
	2 animal  you may gather 1 beast
	2 plant 3 animal  1 damange per beast
	2 animal  you may push up to 2 beast
	Innate - Frenzied Assult  => slow range 1, must have beast
	1 moon 1 fire 4 animal   1 fear and 2 damage,  remove 1 beast
	1 moon 2 fire 5 animal   +1 fear and +1 damange
	Special Rules - Ally of the Beasts - Your presensee may move with beast.  
	Call Forth Predators - During each spirit phase, you may replace 1 of your presense with 1 beast.  The replace presense leaves the game 
	Set Up - put 1 presense and 1 beast in hightest numbered jungle.  Put 1 presense in a land of your choice with beast anywhere on the island.

	Prey on the Builders => 1 => fast, range 0, any => moon, fire, animal => you may gather 1 beast.  If target land has beast, invaders do not ubild there this turn.
	Teeth Gleam From Darkness => 1 => slow, withing 1 of presense in jungle, no blight => moon, plant, animal => (1 fear.  Add 1 beast) -OR- If target land has both beast and invaders: 3 fear
	Too Near the Jungle => 0 => slow, within 1 of presense in jungle, any => plant, animal => 1 fear. destroy 1 dahan
	Terrifying Chase => 1 => slow, range 0, any => sun, animal => Push 2 explorer/town/dahan.  Push another 2 explorer/down/dahan per beast in target land.  If you pushed any invaders, 2 fear


	 */
	public class SharpFangs : Spirit {

		public override string Text => "Sharp Fangs";

		public override GrowthOption[] GetGrowthOptions() {
			static bool beastOrJungle(Space s, GameState gameState) => s.Terrain==Terrain.Jungle || gameState.HasBeasts(s);
		
			var beastOrJungleRange3 = new PlacePresence(3, beastOrJungle);

			return new GrowthOption[]{
				NewGrowthOption(
					new ReclaimAll()       // A
					,new GainEnergy(-1)   // A
					,new DrawPowerCard(1) // A
					,beastOrJungleRange3 // B
				)
				,NewGrowthOption(
					new ReclaimAll()       // A
					,new DrawPowerCard(2) // A & C
					// Engergy Change of 0     // A & C cancel each other out
				)
				,NewGrowthOption(
					new ReclaimAll()       // A
					,new DrawPowerCard(1) // A
					,new GainEnergy(2)    // A + D
				)
				,NewGrowthOption(
					beastOrJungleRange3 // B
					,new GainEnergy(1)  // C
					,new DrawPowerCard(1) // C
				)
				,NewGrowthOption(
					beastOrJungleRange3 // B
					,new GainEnergy(3)  // C
				)
				,NewGrowthOption(
					new DrawPowerCard(1) // C
					,new GainEnergy(1+3)  // C + D
				)
			};
		}

		GrowthOption NewGrowthOption(params GrowthAction[] actions) {
			//	2 2 3 reclaim-1 4 5&reclaim-1
			if( RevealedCardSpaces<4 ) return new GrowthOption(actions);
			var list = actions.ToList();
			list.Add(new Reclaim1());
			if(RevealedCardSpaces==6)
				list.Add(new Reclaim1());
			return new GrowthOption(list.ToArray());
		}

		//	1 animal plant 2 animal 3 4
		protected override int[] EnergySequence => new int[]{1,1,1,2,2,3,4};
		//	2 2 3 reclaim-1 4 5&reclaim-1
		protected override int[] CardSequence => new int[]{2,2,3,3,4,5};

		public override int Elements( Element e ) {
			int bonus = e switch{
				Element.Animal => RevealedEnergySpaces >=5 ? 2 : RevealedEnergySpaces >= 2 ? 1 : 0,
				Element.Plant => RevealedEnergySpaces >=3 ? 1 : 0,
				_ => 0
			};
			return base.Elements( e ) + bonus;
		}

	}
}
