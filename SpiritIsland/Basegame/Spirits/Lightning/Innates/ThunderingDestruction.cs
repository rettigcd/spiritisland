using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any

	[InnatePower( ThunderingDestruction.Name, Speed.Slow )]
	[FromSacredSite(1)]
	class ThunderingDestruction {

		public const string Name = "Thundering Destruction";

		// 3 fire 2 air    destroy 1 town
		[InnateOption("3 fire, 2 air")]
		public static Task Destroy_Town( TargetSpaceCtx ctx ) {
			return DestroyTowns( ctx, 1 );
		}

		// 4 fire 3 air    you may instead destroy 1 city
		[InnateOption("4 fire, 3 air")]
		public static Task Destory_TownOrCity( TargetSpaceCtx ctx ) {
			return DestroyTownsOrCities( ctx, 1 );
		}

		// 5 fire 4 air 1 water    also, destroy 1 town or city
		[InnateOption("5 fire, 4 air, 1 water")]
		public static Task Destroy_2TownsOrCities( TargetSpaceCtx ctx ) {
			return DestroyTownsOrCities( ctx, 2 );
		}

		// 5 fire 5 air 2 water    also, destroy 1 town or city
		[InnateOption("5 fire, 5 air, 2 water")]
		public static Task Destory_3TownsOrCities( TargetSpaceCtx ctx ) {
			return DestroyTownsOrCities( ctx, 3 );
		}

		static Task DestroyTowns(TargetSpaceCtx ctx, int count){
			return ctx.InvadersOn(ctx.Target).Destroy( Invader.Town, count );
		}

		static async Task DestroyTownsOrCities(TargetSpaceCtx ctx,int count){
			var grp = ctx.InvadersOn(ctx.Target);
			InvaderSpecific[] invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await ctx.Self.SelectInvader("Select town/city to destroy.",invadersToDestroy,Present.Done);
				if(invader==null) break;
				await grp.Destroy( invader.Generic, 1 );

				// next
				invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
				--count;
			}
		}

	}

}
