﻿using System;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	[InnatePower(DarknessSwallowsTheUnwary.Name,Speed.Fast)]
	[FromSacredSite(1)]
	public class DarknessSwallowsTheUnwary {

		public const string Name = "Darkness Swallows the Unwary";

		[InnateOption("2 moon, 1 fire")]
		public static async Task Gather1Explorer( ActionEngine engine, Space target ) {
			await engine.GatherUpToNInvaders( target, 1, Invader.Explorer );
		}

		[InnateOption("3 moon, 2 fire")]
		public static async Task Plus_Destory2Explorers( ActionEngine engine, Space target){
			await Gather1Explorer(engine,target);

			// destory 2 explorers (+1 fear/kill)
			var grp = engine.GameState.InvadersOn( target );
			int explorersToDestroy = Math.Min( grp[Invader.Explorer], 2 );
			grp.Destroy(Invader.Explorer, explorersToDestroy );
			engine.GameState.UpdateFromGroup( grp );
			engine.GameState.AddFear( explorersToDestroy );
		}

		[InnateOption("3 moon, 2 fire")]
		public static async Task Plus_3Damage( ActionEngine engine, Space target){
			await Plus_Destory2Explorers(engine,target);

			// 3 more points of damage (+ 1 fear/kill )
			int startingCount = engine.GameState.InvadersOn( target ).TotalCount;
			engine.GameState.DamageInvaders( target, 3 );
			int endingCount = engine.GameState.InvadersOn( target ).TotalCount;
			int killed = startingCount - endingCount;
			engine.GameState.AddFear( killed );
		}

	}

}
