namespace SpiritIsland.Basegame;

public class FieldsChokedWithGrowth {

	// push 1 town -OR- push 3 dahan
	[SpiritCard( "Fields Choked with Growth", 0, Element.Sun, Element.Water, Element.Plant )]
	[Slow]
	[FromPresence( 1 )]
	static public Task ActionAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction("Push 1 town", ctx => ctx.Push(1,Invader.Town)).Matches( x => x.Tokens.Has(Invader.Town) ),
			new SpaceAction("Push 3 dahan", ctx => ctx.PushDahan(3)).Matches( x => x.Dahan.Any )
		);

	}

}