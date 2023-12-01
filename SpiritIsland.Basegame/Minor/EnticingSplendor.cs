namespace SpiritIsland.Basegame;

public class EnticingSplendor {

	[MinorCard( "Enticing Splendor", 0, Element.Sun, Element.Air, Element.Plant ),Fast,FromPresence( 0, Filter.NoBlight )]
	[Instructions( "Gather 1 Explorer / Town. -or- Gather up to 2 Dahan." ), Artist( Artists.JoshuaWright )]
	public static Task ActAsync( TargetSpaceCtx ctx) {

		return ctx.SelectActionOption(
			new SpaceCmd( "Gather 1 explorer/town", ctx => ctx.GatherUpTo( 1, Human.Explorer_Town ) ),
			new SpaceCmd( "Gather up to 2 dahan", ctx => ctx.GatherUpToNDahan( 2 ) )
		);

	}

}