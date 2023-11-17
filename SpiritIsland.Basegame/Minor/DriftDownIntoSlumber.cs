namespace SpiritIsland.Basegame;

public class DriftDownIntoSlumber {

	[MinorCard( "Drift Down Into Slumber", 0, Element.Air, Element.Earth, Element.Plant ),Fast,FromPresence( 2 )]
	[Instructions( "Defend 1. If target land is Jungle / Sands, instead, Defend 4." ), Artist( Artists.LucasDurham )]
	public static Task ActAsync( TargetSpaceCtx ctx ) {
		// defend 1
		ctx.Defend( 1 );

		// if target land is J/S, instead defend 4
		if( ctx.IsOneOf( Terrain.Jungle, Terrain.Sands ) )
			ctx.Defend( 4-1 ); // -1 is from defend already done above

		return Task.CompletedTask;
	}
}