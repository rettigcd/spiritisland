namespace SpiritIsland.BranchAndClaw;

public class TooNearTheJungle {

	[SpiritCard( "Too Near the Jungle", 0, Element.Plant, Element.Animal )]
	[Slow]
	[FromPresenceIn( 1, Terrain.Jungle )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		ctx.AddFear(1);
		await ctx.Invaders.DestroyNOfAnyClass(1,Invader.Explorer);
	}

}