﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class LightningsBoon {
		public const string Name = "Lightning's Boon";

		[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			return new SpeedChanger( ctx.Other, ctx.GameState, Speed.Fast, 2 ).Exec();
		}

	}

}
