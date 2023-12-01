namespace SpiritIsland.Basegame;

public class LureOfTheUnknown {

	[MinorCard( "Lure of the Unknown", 0, Element.Moon, Element.Fire, Element.Air, Element.Plant ),Fast,FromPresence( 2, Filter.NoInvader )]
	[Instructions( "Gather 1 Explorer / Town" ), Artist( Artists.LucasDurham )]
	public static Task ActAsync( TargetSpaceCtx ctx ) {
		// Gather 1 explorer or town
		return ctx.GatherUpTo( 1, Human.Explorer_Town );
	}

}