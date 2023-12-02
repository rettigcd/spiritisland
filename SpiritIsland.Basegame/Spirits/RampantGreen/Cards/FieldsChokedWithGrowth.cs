namespace SpiritIsland.Basegame;

public class FieldsChokedWithGrowth {

	[SpiritCard( "Fields Choked With Growth", 0, Element.Sun, Element.Water, Element.Plant ),Slow,FromPresence( 1 )]
	[Instructions("Push 1 Town. -or- Push 3 Dahan" ),Artist(Artists.JorgeRamos)]
	static public Task ActionAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction("Push 1 town", ctx => ctx.Push(1,Human.Town)).OnlyExecuteIf( x => x.Tokens.Has(Human.Town) ),
			new SpaceAction("Push 3 dahan", ctx => ctx.PushDahan(3)).OnlyExecuteIf( x => x.Dahan.Any )
		);

	}

}