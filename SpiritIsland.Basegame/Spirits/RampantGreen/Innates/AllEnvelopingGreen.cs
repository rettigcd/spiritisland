﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	//1 water 3 plant defend 2
	//2 water 4 plant instead, defend 4
	//3 water 1 rock 5 plant also, remove 1 blight

	[InnatePower("All Enveloping Green",Speed.Fast)]
	[FromSacredSite(1)]
	class AllEnvelopingGreen {

		[InnateOption("1 water,3 plant","Defend 2.")]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			//defend 2
			ctx.Defend(2);
			return Task.CompletedTask;
		}

		[InnateOption( "2 water,4 plant","Instead, Defend 4." )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			//defend 4 (instead)
			ctx.Defend(4);
			return Task.CompletedTask;
		}

		[InnateOption( "3 water,5 plant,1 earth", "Also, remove 1 blight." )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			Option2Async(ctx);
			// also remove 1 blight
			ctx.RemoveBlight();
			return Task.CompletedTask;
		}

	}
}