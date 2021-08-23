﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland.BranchAndClaw {

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

		static Track FivePlaysReclaim1() {
			var track = Track.MkCard(5);
			track.ReclaimOne = true;
			return track;
		}

		public SharpFangs():base(
			new MyPresence(
				new Track[] { Track.Energy1, Track.AnimalEnergy, Track.PlantEnergy, Track.Energy2, Track.AnimalEnergy, Track.Energy3, Track.Energy4 },
				new Track[] { Track.Card2, Track.Card2, Track.Card3, Track.Reclaim1, Track.Card4, FivePlaysReclaim1() }
			),
			new NullPowerCard( "A", 0, Speed.Fast ),
			new NullPowerCard( "B", 0, Speed.Fast ),
			new NullPowerCard( "C", 0, Speed.Fast ),
			new NullPowerCard( "D", 0, Speed.Fast )
		) {
		
			var beastOrJungleRange3 = new PlacePresence(3, Target.BeastOrJungle,"beast or jungle");

			var a = new GrowthActionFactory[]{
				new ReclaimAll()       // A
				,new GainEnergy(-1)   // A
				,new DrawPowerCard(1) // A
			};

			var b= new GrowthActionFactory[]{
				beastOrJungleRange3
			};

			var c = new GrowthActionFactory[]{
				new DrawPowerCard(1) // C
				,new GainEnergy(1)   // C
			};

			var d = new GrowthActionFactory[]{
				new GainEnergy(3)   // D
			};

			static GrowthOption Join(GrowthActionFactory[] a,GrowthActionFactory[] b) 
				=> new GrowthOption( a.Union(b).ToArray() );

			GrowthOptions = new GrowthOption[]{
				 Join(a,c) // -1+1
				,Join(a,d) // -1+3
				,Join(b,c) // +1
				,Join(b,d) // +3
				,Join(c,d) // +1 + 3
				,Join(a,b) // -1 
			};

		}

		public override void Grow( GameState gameState, int optionIndex ) {

			var actions = this.GetGrowthOptions()[optionIndex].GrowthActions;
			foreach(var action in actions.Take(5))
				AddActionFactory( action );

			// costs 1
			if(1 <= Energy)
				AddActionFactory( actions[5] );

		}

		protected override void InitializeInternal( Board _, GameState _1 ) {
			throw new System.NotImplementedException();
		}

	}

}
