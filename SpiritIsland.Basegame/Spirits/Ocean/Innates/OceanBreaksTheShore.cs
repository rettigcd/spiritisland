namespace SpiritIsland.Basegame;

[InnatePower( "Ocean Breaks the Shore" ),Slow]
[FromPresence( 0, Filter.Coastal )]
public class OceanBreaksTheShore {

		
	[InnateTier( "2 water,1 earth","Drown 1 town." )]
	static public Task Option1( TargetSpaceCtx ctx ) {
		// drown 1 town
		return ctx.Invaders.DestroyNOfClass(1,Human.Town);
	}

	[InnateTier( "3 water,2 earth","You may instead Drown 1 city." )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		// instead drown 1 city
		return ctx.Invaders.DestroyNOfAnyClass( 1, Human.Town_City );
	}

	[InnateTier( "4 water,3 earth", "Also, Drown 1 town/city." )]
	static public Task Option3( TargetSpaceCtx ctx ) {
		// also drown 1 town or city
		return ctx.Invaders.DestroyNOfAnyClass( 2, Human.Town_City );
	}

}
