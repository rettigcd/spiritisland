namespace SpiritIsland.JaggedEarth;

public class SoftlyBeckonEverInward {

	[SpiritCard("Softly Beckon Ever Inward",2,Element.Moon,Element.Air),Slow,FromPresence(0,Target.Inland)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		await ctx.Gatherer
			// gather up to 2 explorers
			.AddGroup(2,Invader.Explorer)
			// gather up to 2 towns
			.AddGroup(2,Invader.Town)
			// gather up to 2 beast
			.AddGroup(2,TokenType.Beast)
			// gather up to 2 dahan
			.AddGroup(2,TokenType.Dahan)
			.GatherUpToN();

	}

}