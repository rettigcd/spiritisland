namespace SpiritIsland.JaggedEarth;

public class UnexpectedTigers {

	[SpiritCard("Unexpected Tigers",0,Element.Moon,Element.Fire,Element.Animal), Slow, FromPresence(1) ]
	static public async Task ActAsymc(TargetSpaceCtx ctx ) { 
		// 1 fear if invaders are present.
		if( ctx.HasInvaders )
			ctx.AddFear(1);

		var beastSources = ctx.Adjacent.Where(s=>ctx.Target(s).Beasts.Any).ToArray();
		// If you can gather 1 beast,
		if( beastSources.Length > 0) {
			// do so,
			await ctx.Gather(1,TokenType.Beast);
			// then push 1 explorer.
			await ctx.Push(1,Invader.Explorer);
		} else {
			// othersie, add 1 beast
			await ctx.Beasts.Add(1);
		}

	}

}