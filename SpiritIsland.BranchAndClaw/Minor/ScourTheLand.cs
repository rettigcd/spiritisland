namespace SpiritIsland.BranchAndClaw;

public class ScourTheLand {

	[MinorCard( "Scour the Land", 1, Element.Air, Element.Earth ),SlowButFastIf("3 air"), FromPresence(2)]
	[Instructions( "Destroy 3 Town and all Explorer. Add 1 Blight. -If you have- 3 Air: This power may be Fast." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.Invaders.DestroyNOfClass(3,Human.Town);
		await ctx.Invaders.DestroyAll(Human.Explorer);

		await ctx.AddBlight(1);

	}

}