namespace SpiritIsland.BranchAndClaw;

public class GoldsAllure {

	[MinorCard("Gold's Allure",0,Element.Fire,Element.Earth,Element.Animal),Slow,FromPresence(1,Filter.Mountain)]
	[Instructions( "Gather 1 Explorer. and 1 Town. Add 1 Strife." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		await ctx.Gather(1,Human.Explorer);
		await ctx.Gather(1,Human.Town);
		await ctx.AddStrife();
	}

}