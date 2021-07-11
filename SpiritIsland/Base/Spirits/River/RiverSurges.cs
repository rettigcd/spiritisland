using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	/*

		=================================================
		River Surges In Sunlight

		* reclaim, +1 power card, +1 energy
		* +1 presense withing 1, +1 presense range 1
		* +1 power card, +1 presense range 2

		1 2 2 3 4 4 5
		1 2 2 3 reclaim-1 4 5
		Innate: Massive Flooding => slow, 1 from sacred, any
		1 sun 2 water   push 1 emplore or town
		2 sun 3 water   instead, 2 damange. push up to 3 explorers or towns
		Special Rules: Rivers Domain - Your presense in wetlands count as sacred
		setup - put 1 presense on your starting board in the highest number wetlands

		Boon of Vigor => 0 => fast,any spirit		=> sun, water, plant	=> If you target yourself, gain 1 energy.  If you target another spirit, they gain 1 energy per power card they played this turn
		River's Bounty => 0 => slow, range 0, any	=> sun, water, animal	=> gather up to 2 dahan.  If ther are now at least 2 dahan, add 1 dahan and gain +1 energy
		Wash Away => 1 => slow, range 1, any		=> water mountain		=> Push up to 3 explorers / towns
		Flash Floods => 2 => fast, range 1, any		=> sun, water			=> 1 Damange.  If target land is costal +1 damage.

		Power Progression => 
		1. Uncanny Melting => 1, slow, range 1 from ss, any => sun, moon, water => if invaders present then 1 fear.  If target land is S/W then remove 1 blight
		2. Nature's Resilience => 1, fast, range 1 from ss => earth, plant, animal => defend 6, if you have 2 water, may remove blight instead
		3. Pull Beneath the Hungry Earth => 1 slow, range:1 => moon, water, earth => if target has your presence then 1 fear and 1 damage.  if target land is S/W then 1 damage
		4. Accelerated Rot (Major) => 4 slow, range 2, target J/W => sun, water, plant => 2 fear. 4 damage. If you have 3 sun, 2 water, 3 plant, then +5 damage. Remove 1 blight
		5. Song of Sanctity => 1 slow, range 1 (M/J), sun, water, plant => if target has explorer, push all explorer. OTHERWISE remove 1 blight
		6. Tsunami (Major) => 6 slow, range 2 from ss (costal), water, earth => 2 fear, 8 damage, destroy 2 dahan.  if you have 3 water and 2 earth then in eath OTHER costal on same board->1 fear, 4 damage and destroy 1 dahan
		7. Encompassing Ward => 1 fast target spirit => sun water earth => defend 2 in every land where target spirit has presense

		*/

	/// <summary>
	/// River Surges in Sunlight
	/// </summary>
	public class RiverSurges : Spirit {

		public const string Name = "River Surges in Sunlight";

		public override string Text => Name;

		public RiverSurges():base(
			PowerCard.For(BoonOfVigor.ActionAsync),
			PowerCard.For(FlashFloods.ActionAsync),
			PowerCard.For(RiversBounty.ActionAsync),
			PowerCard.For(WashAway.ActionAsync)
		){
			GrowthOptions =  new GrowthOption[]{ 
				new GrowthOption(
					new ReclaimAll(),
					new DrawPowerCard(1),
					new GainEnergy(1)
				),
				new GrowthOption(
					new PlacePresence( 1 ),
					new PlacePresence( 1 )
				),
				new GrowthOption( 
					new DrawPowerCard( 1 ),
					new PlacePresence( 2 ) 
				)
			};

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<MassiveFlooding>()
			};
		}

		bool Reclaim1FromCardTrack => this.RevealedCardSpaces >= 5;
		protected override int[] EnergySequence => new int[]{1, 2, 2, 3, 4, 4, 5 };
		protected override int[] CardSequence => new int[]{1, 2, 2, 3, 3, 4, 5 };


		public override int RevealedCardSpaces {
			get{ return base.RevealedCardSpaces; }
			set{
				// this never happens during Reclaim-all so is always ok to add here
				bool hadReclaim1 = Reclaim1FromCardTrack;
				base.RevealedCardSpaces = value;
				if(!hadReclaim1 && Reclaim1FromCardTrack)
					AddActionFactory(new Reclaim1());
			}
		}

		#region growth

		public override void Grow(GameState gameState, int optionIndex) {
			if( optionIndex != 0 // Reclaim-All
				&& Reclaim1FromCardTrack
			)
				AddActionFactory(new Reclaim1());
			base.Grow(gameState, optionIndex);
		}


		#endregion

		public override IEnumerable<Space> SacredSites => Presence
			.Where(s=>s.Terrain==Terrain.Wetland)
			.Union( base.SacredSites )
			.Distinct();

		public override void AddActionFactory(IActionFactory actionFactory) {

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
			PowerCard.For<UncannyMelting>(),
			PowerCard.For<NaturesResilience>(),
			PowerCard.For<PullBeneathTheHungryEarth>(),
			PowerCard.For<AcceleratedRot>(),  // MAJOR?
			PowerCard.For<SongOfSanctity>(),
			PowerCard.For<Tsunami>(),
			PowerCard.For<EncompassingWard>()
		};

		public override void InitializePresence( Board board ) {
			var space = board.Spaces.Reverse().First(s=>s.Terrain==Terrain.Wetland);
			Presence.Add(space);
		}
	}

}
