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
		var ss = new SourceSelector(ctx.Tokens)
			.AddAll(Human.Invader)
			.FilterSpaceToken( st => 0 < st.Token.AsHuman().StrifeCount );
		await ctx.Tokens.Invaders.UserSelectedDamageAsync( ctx.Self, await ctx.Self.Elements.GetAsync(Element.Sun), ss, Present.Always );
	}

	[InnateTier("1 sun,1 moon,4 air","For each sun moon pair you have, Destroy 1 Invader with Strife.",2)]
	public static async Task Option3(TargetSpaceCtx ctx ) {
		int count = Math.Min( await ctx.Self.Elements.GetAsync(Element.Sun), await ctx.Self.Elements.GetAsync(Element.Moon) );

		SourceSelector ss = new SourceSelector(ctx.Tokens)
			.FilterSpaceToken( st => 0 < st.Token.AsHuman().StrifeCount )
			.AddGroup( count, Human.Invader );

		while(true) {
			var st = await ss.GetSource(ctx.Self,"Destroy", Present.Always);
			if(st==null) break;
			await st.Destroy();
		}
		
	}

}