﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any

	[InnatePower( ThunderingDestruction.Name, Speed.Slow )]
	[FromSacredSite(1)]
	class ThunderingDestruction {

		public const string Name = "Thundering Destruction";

		// 3 fire 2 air    destroy 1 town
		[InnateOption("3 fire, 2 air")]
		public static Task Destroy_Town( TargetSpaceCtx ctx ) {
			return ctx.PowerInvaders.Destroy( 1, Invader.Town );
		}

		// 4 fire 3 air    you may instead destroy 1 city
		[InnateOption("4 fire, 3 air")]
		public static Task Destory_TownOrCity( TargetSpaceCtx ctx ) {
			return ctx.PowerInvaders.DestroyAny( 1, Invader.Town, Invader.City );
		}

		// 5 fire 4 air 1 water    also, destroy 1 town or city
		[InnateOption("5 fire, 4 air, 1 water")]
		public static Task Destroy_2TownsOrCities( TargetSpaceCtx ctx ) {
			return ctx.PowerInvaders.DestroyAny( 2, Invader.Town, Invader.City );
		}

		// 5 fire 5 air 2 water    also, destroy 1 town or city
		[InnateOption("5 fire, 5 air, 2 water")]
		public static Task Destory_3TownsOrCities( TargetSpaceCtx ctx ) {
			return ctx.PowerInvaders.DestroyAny( 3, Invader.Town, Invader.City );
		}

	}

}
