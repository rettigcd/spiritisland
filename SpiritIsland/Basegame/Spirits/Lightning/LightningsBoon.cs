﻿using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class LightningsBoon {
		public const string Name = "Lightning's Boon";

		[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task ActAsync(ActionEngine _,Spirit target) {
			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			await target.SelectActionsAndMakeFast( 2 );
		}

	}

}
