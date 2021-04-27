using System;
using System.Collections.Generic;
using System.Text;

namespace SpiritIsland {

/*
===================================================
Lightning's Swift Strike

 * reclaim, +1 power card, +1 energy
* +1 presence range 2, +1 presence range 0
* +1 presense range 1, +3 energy

1 1 2 2 3 4 4 5
2 3 4 5 6
Innate:  Thundering Destruction => slow, 1 from sacred, any
3 fire 2 air    destroy 1 town
4 fire 3 air    you may instead destroy 1 city
5 fire 4 air 1 water    also, destroy 1 town or city
5 fire 5 air 2 water    also, destroy 1 town or city
Special Rules: Switftnes of Lightning - for every air you have, you may use 1 slow power as if it were fast (cards or innate)
Setup: put 2 pressence in highest numbered sands

Ligning's Boon => 1 => fast, any spirt => fire, air => Taret spirit may use up to 2 powers as if they were fast powers this turn.
Harbinger of the Lighning => 0 => slow, range 1, any => fire, air => Push up to 2 dahan.  1 fear if you pushed any dahan into a land with town or city.
Shatter homesteads => 2 => slow, range 2 from sacred site, any => fire, air => 1 fear.  Destroy 1 town
Raging Storm => 3 => slow, range 1, any => fire, air, water => 1 damange to each invader.

*/

	public class Lightning : Spirit {
		public override GrowthOption[] GetGrowthOptions() {
			return new GrowthOption[]{
				new GrowthOption( new ReclaimAll(), new DrawPowerCard(1), new GainEnergy(1) ),
				new GrowthOption( new PlacePresence(0), new PlacePresence(2) ), // +1 presence range 2, +1 presence range 0
				new GrowthOption( new GainEnergy(3), new PlacePresence(1) ), // +1 presense range 1, +3 energy
			};
		}
	}
}
