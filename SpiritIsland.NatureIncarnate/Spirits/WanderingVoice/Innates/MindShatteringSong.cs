namespace SpiritIsland.NatureIncarnate;

[InnatePower("MindShatteringSong")]
[Slow,FromSacredSite(1,Target.Strife)]
public class MindShatteringSong {

	[InnateTier("1 moon, 2 air","1 Fear per moon you have.",0)]
	public static async Task Option1(TargetSpaceCtx ctx ) {
		int moonCount = await ctx.Self.Elements.GetAsync(Element.Moon);
		ctx.AddFear( moonCount );
	}

	[InnateTier("1 sun,2 air","1 Damage per sun you have to Invaders with Strife only.",1)]
	public static async Task Option2(TargetSpaceCtx ctx ) {
		var ss = ctx.SourceSelector
			.AddAll(Human.Invader)
			.FilterSpaceToken( st => 0 < st.Token.AsHuman().StrifeCount );
		await ss
			.DoDamage( ctx.Self, await ctx.Self.Elements.GetAsync(Element.Sun), Present.Always );
	}

	[InnateTier("1 sun,1 moon,4 air","For each sun moon pair you have, Destroy 1 Invader with Strife.",2)]
	public static async Task Option3(TargetSpaceCtx ctx ) {
		int count = Math.Min( await ctx.Self.Elements.GetAsync(Element.Sun), await ctx.Self.Elements.GetAsync(Element.Moon) );

		var spaceTokens = ctx.SourceSelector
			.AddGroup( count, Human.Invader )
			.FilterSpaceToken( st => 0 < st.Token.AsHuman().StrifeCount )
			.GetEnumerator(ctx.Self, Prompt.RemainingParts("Destroy"), Present.Always);

		await foreach( SpaceToken st in spaceTokens )
			await st.Destroy();
	
	}

}