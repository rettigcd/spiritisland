using System;
using System.Collections.Generic;
using System.Text;

namespace SpiritIsland {

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

	Boon of Vigor => 0 => fast,any spirit => sun, water, plant => If you target yourself, gain 1 energy.  If you target another spirit, they gain 1 energy per power card they played this turn
	Flash Floods => 2 => fast, range 1, any => sun, water => 1 Damange.  If target land is costal +1 damage.
	Wash Away => 1 => slow, range 1, any => water mountain => Push up to 3 explorers / towns
	River's Bounty => 0 => slow, range 0, any => sun, water, animal => gather up to 2 dahan.  If ther are now at least 2 dahan, add 1 dahan and gain +1 energy

	*/

	/// <summary>
	/// River Surges in Sunlight
	/// </summary>
	public class RiverSurges : Spirit {

		bool Reclaim1FromCardTrack => this.RevealedCardSpaces >= 5;
		protected override int[] EnergySequence => new int[]{1, 2, 2, 3, 4, 4, 5 };
		protected override int[] CardSequence => new int[]{1, 2, 2, 3, 3, 4, 5 };



		public override GrowthOption[] GetGrowthOptions(GameState _) {

			return new GrowthOption[]{
				new GrowthOption(
					new ReclaimAll(this),
					new DrawPowerCard(this,1),
					new GainEnergy(this,1)
				),
				Get2PresenceGrowthOption(),
				GetPowerAndPresenceGrowthOption(),
			};
		}

		GrowthOption GetPowerAndPresenceGrowthOption() {
			var actions = new List<GrowthAction>{
				new DrawPowerCard( this, 1 ),
				new PlacePresence( this, 2 )
			};
			if( Reclaim1FromCardTrack )
				actions.Add(new Reclaim1(this));
			return new GrowthOption( actions );
		}

		GrowthOption Get2PresenceGrowthOption() {
			var actions = new List<GrowthAction>{
				new PlacePresence( this, new RangeCriteria( 1 ) ),
				new PlacePresence( this, new RangeCriteria( 1 ) )
			};
			if( Reclaim1FromCardTrack )
				actions.Add(new Reclaim1(this));
			return new GrowthOption( actions );
		}
	}
}
