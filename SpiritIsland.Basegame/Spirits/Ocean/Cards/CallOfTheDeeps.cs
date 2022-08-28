namespace SpiritIsland.Basegame;

public class CallOfTheDeeps {

	[SpiritCard("Call of the Deeps",0,Element.Moon,Element.Air,Element.Water)]
	[Fast]
	[FromPresence(0,Target.Coastal)]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		// Gather 1 explorer, if target land is the ocean, you may gather another explorer
		int count = ctx.Is(Terrain.Ocean) ? 2 : 1;
		return ctx.GatherUpTo(count,Invader.Explorer);

	}

}