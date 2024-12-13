namespace SpiritIsland.BranchAndClaw;

public class FireInTheSky {

	[MinorCard("Fire in the Sky",1,Element.Sun,Element.Fire,Element.Air),Fast,FromSacredSite(1)]
	[Instructions( "2 Fear. Add 1 Strife" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		await ctx.AddFear(2);
		await ctx.AddStrife();
	}

}