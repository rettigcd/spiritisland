﻿using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

/*
===================================================
Lightning's Swift Strike

 * reclaim, +1 power card, +1 energy
* +1 presence range 2, +1 presence range 0
* +1 presense range 1, +3 energy

1 1 2 2 3 4 4 5
2 3 4 5 6

Special Rules: Switftnes of Lightning - for every air you have, you may use 1 slow power as if it were fast (cards or innate)

Ligning's Boon => 1 => fast, any spirt => fire, air => Taret spirit may use up to 2 powers as if they were fast powers this turn.
Harbinger of the Lighning => 0 => slow, range 1, any => fire, air => Push up to 2 dahan.  1 fear if you pushed any dahan into a land with town or city.
Shatter homesteads => 2 => slow, range 2 from sacred site, any => fire, air => 1 fear.  Destroy 1 town
Raging Storm => 3 => slow, range 1, any => fire, air, water => 1 damange to each invader.

*/

	public class LightningsSwiftStrike : Spirit {
		public const string Name = "Lightning's Swift Strike";

		public LightningsSwiftStrike():base(
			PowerCard.For<HarbingersOfTheLightning>(),
			PowerCard.For<LightningsBoon>(),
			PowerCard.For<RagingStorm>(),
			PowerCard.For<ShatterHomesteads>()
		){
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( 
					new ReclaimAll(), 
					new DrawPowerCard(1), 
					new GainEnergy(1)
				),
				// +1 presence range 2, +1 presence range 0( 
				new GrowthOption(
					new PlacePresence(2),
					new PlacePresence(0) 
				),
				// +1 presense range 1, +3 energy
				new GrowthOption( new GainEnergy(3), new PlacePresence(1) ),
			};

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<ThunderingDestruction>()
			};

		}

		public override string Text => Name;

		protected override int[] EnergySequence => new int[]{ 1, 1, 2, 2, 3, 4, 4, 5 };
		protected override int[] CardSequence => new int[]{ 2, 3, 4, 5, 6 };

		public override void ActivateAvailableCards( params PowerCard[] cards ) {
			base.ActivateAvailableCards( cards );
			swiftness.OnActivateCards( this );
		}
		static readonly SwiftnessOfLightning swiftness = new SwiftnessOfLightning();

		public override void AddActionFactory(IActionFactory actionFactory) {
			// !!! duplicated in River

			if(actionFactory is DrawPowerCard){
				var newCard = PowerProgression[0];
				this.Hand.Add( newCard );
				PowerProgression.RemoveAt( 0 );
				if(newCard.PowerType == PowerType.Major)
					base.AddActionFactory(new ForgetPowerCard());
			} else
				base.AddActionFactory(actionFactory);
		}

		readonly List<PowerCard> PowerProgression = new List<PowerCard>{
			PowerCard.For<DelusionsOfDanger>(),
			PowerCard.For<CallToBloodshed>(),
			PowerCard.For<PowerStorm>(),
			PowerCard.For<PurifyingFlame>(),
			PowerCard.For<PillarOfLivingFlame>(),
			PowerCard.For<EntrancingApparitions>(),
			PowerCard.For<CallToIsolation>()
		};

		public override void InitializePresence( Board board ) {
			// Setup: put 2 pressence in highest numbered sands
			var space = board.Spaces.Reverse().First(x=>x.Terrain==Terrain.Sand);
			Presence.Add(space);
			Presence.Add(space);
		}


	}
}
