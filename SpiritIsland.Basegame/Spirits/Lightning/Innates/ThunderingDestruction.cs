namespace SpiritIsland.Basegame;

[InnatePower( ThunderingDestruction.Name),Slow]
[FromSacredSite(1)]
public class ThunderingDestruction {

	public const string Name = "Thundering Destruction";

	// 3 fire 2 air    destroy 1 town
	[InnateOption("3 fire, 2 air", "Destroy 1 town." )]
	public static Task Destroy_Town( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfClass( 1, Human.Town );
	}

	// 4 fire 3 air    you may instead destroy 1 city
	[InnateOption("4 fire, 3 air", "You may instead destroy 1 city." )]
	public static Task Destroy_TownOrCity( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfAnyClass( 1, Human.Town_City );
	}

	// 5 fire 4 air 1 water    also, destroy 1 town or city
	[InnateOption("5 fire,4 air,1 water", "Also, Destroy 1 town/city." )]
	public static Task Destroy_2TownsOrCities( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfAnyClass( 2, Human.Town_City );
	}

	// 5 fire 5 air 2 water    also, destroy 1 town or city
	[InnateOption( "5 fire, 5 air, 2 water", "Also, Destroy 1 town / city." )]
	public static Task Destroy_3TownsOrCities( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfAnyClass( 3, Human.Town_City );
	}

}