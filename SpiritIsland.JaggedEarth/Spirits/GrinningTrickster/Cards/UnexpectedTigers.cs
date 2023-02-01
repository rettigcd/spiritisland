namespace SpiritIsland.JaggedEarth;

public class UnexpectedTigers {

	[SpiritCard("Unexpected Tigers",0,Element.Moon,Element.Fire,Element.Animal), Slow, FromPresence(1) ]
	static public async Task ActAsymc(TargetSpaceCtx ctx ) { 
		// 1 fear if invaders are present.
		if( ctx.HasInvaders )
			ctx.AddFear(1);

		// If you can gather 1 beast,
		if( ctx.Adjacent.Any(s=>s.Beasts.Any) ) {
			// do so,
			await ctx.Gather(1,Token.Beast);
			// then push 1 explorer.
			await ctx.Push(1,Human.Explorer);
		} else {
			// othersie, add 1 beast
			await ctx.Beasts.Add(1);
		}

	}

}