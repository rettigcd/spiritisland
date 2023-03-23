namespace SpiritIsland.Basegame;

public class CallOfTheDeeps {

	[SpiritCard("Call of the Deeps",0,Element.Moon,Element.Air,Element.Water),Fast,FromPresence(0,Target.Coastal)]
	[Instructions( "Gather 1 Explorer. If target land is the Ocean, you may Gather another Explorer." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		// Gather 1 explorer, if target land is the ocean, you may gather another explorer
		int count = ctx.Is(Terrain.Ocean) ? 2 : 1;
		return ctx.GatherUpTo(count,Human.Explorer);

	}

}