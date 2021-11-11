﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Creepers Tear into Mortar" ),Slow, FromPresence( 0 )]
	[RepeatIf("2 moon,3 plant","3 moon,4 plant")]
	class CreepersTearIntoMortar {

		[InnateOption( "1 moon,2 plant", "1 Damage to 1 town/city." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			return ctx.DamageInvaders(1,Invader.City, Invader.Town);
		}

	}

}