﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Beset and Confound the Invaders"), Fast, FromPresence(2, Target.Invaders)]
	public class BesetAndConfoundTheInvaders {

		[InnateOption("1 air,2 animal","2 beast - 2 fear and Defend 2")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			if(2 <= ctx.Beasts.Count) {
				ctx.AddFear(2);
				ctx.Defend(2);
			}
			return Task.CompletedTask;
		}

		// 2 air 3 animal 3 beast - Instead, 3 fear and defend 4
		[InnateOption("2 air,3 animal","3 beast - Instead, 3 fear and Defend 2")]
		static public Task Option2(TargetSpaceCtx ctx ) {
			if(3 <= ctx.Beasts.Count) {
				ctx.AddFear(3);
				ctx.Defend(4);
				return Task.CompletedTask;
			} else 
				return Option1(ctx);
		}

		// 3 air 4 animal 4 beast - Instead, 4 fear and defend 7
		[InnateOption("3 air,4 animal","4 beast - Instead, 4 fear and Defend 7")]
		static public Task Option3(TargetSpaceCtx ctx ) {
			if(4 <= ctx.Beasts.Count) {
				ctx.AddFear(4);
				ctx.Defend(7);
				return Task.CompletedTask;
			} else 
				return Option2(ctx);
		}

		// 4 air 1 earth 5 animal 5 beast - Instead, 6 fear and Defend 10.
		[InnateOption("4 air,1 earth,5 animal","5 beast - Instead, 6 fear and Defend 10")]
		static public Task Option4(TargetSpaceCtx ctx ) {
			if(4 <= ctx.Beasts.Count) {
				ctx.AddFear(6);
				ctx.Defend(10);
				return Task.CompletedTask;
			} else 
				return Option3(ctx);
		}

	}


}
