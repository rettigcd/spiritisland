namespace SpiritIsland.Basegame;

public class DreamsOfTheDahan {

	[SpiritCard("Dreams of the Dahan",0,Element.Moon,Element.Air), Fast, FromPresence(2)]
	[Instructions("Gather up to 2 Dahan. -or- If target land has Town / City, 1 Fear for each Dahan, to a maximum of 3 Fear."),Artist( Artists.ShaneTyree)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceCmd( "Gather up to 2 dahan", ctx => ctx.GatherUpToNDahan( 2 ) ),
			new SpaceCmd( "1 fear/dahan, max 3", ctx => ctx.AddFear(Math.Min(3,ctx.Dahan.CountAll)))
				.OnlyExecuteIf( x => x.Tokens.HasAny( Human.Town_City ) )
		);
	}

}