﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any

	[InnatePower( ThunderingDestruction.Name, Speed.Slow )]
	[FromSacredSite(1)]
	class ThunderingDestruction {

		public const string Name = "Thundering Destruction";

		// 3 fire 2 air    destroy 1 town
		[InnateOption("3 fire, 2 air")]
		public static Task Destroy_Town( ActionEngine engine, Space target ) {
			return DestroyTowns( engine, target, 1 );
		}

		// 4 fire 3 air    you may instead destroy 1 city
		[InnateOption("4 fire, 3 air")]
		public static Task Destory_TownOrCity( ActionEngine engine, Space target ) {
			return DestroyTownsOrCities( engine, target, 1 );
		}

		// 5 fire 4 air 1 water    also, destroy 1 town or city
		[InnateOption("5 fire, 4 air, 1 water")]
		public static Task Destroy_2TownsOrCities( ActionEngine engine, Space target ) {
			return DestroyTownsOrCities( engine, target, 2 );
		}

		// 5 fire 5 air 2 water    also, destroy 1 town or city
		[InnateOption("5 fire, 5 air, 2 water")]
		public static Task Destory_3TownsOrCities( ActionEngine engine, Space target ) {
			return DestroyTownsOrCities( engine, target, 3 );
		}

		static Task DestroyTowns(ActionEngine engine, Space target, int count){
			engine.GameState.InvadersOn(target)
				.Destroy( Invader.Town, count );
			return Task.CompletedTask;
		}

		static async Task DestroyTownsOrCities(ActionEngine engine,Space target,int count){
			var gameState = engine.GameState;
			var grp = gameState.InvadersOn(target);
			// !!! only show City/town, not individual health points
			var invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await engine.SelectInvader("Select town/city to destroy.",invadersToDestroy,true);
				if(invader==null) break;
				grp.Destroy( invader.Generic, 1 );

				// next
				invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
				--count;
			}
		}

	}

}
