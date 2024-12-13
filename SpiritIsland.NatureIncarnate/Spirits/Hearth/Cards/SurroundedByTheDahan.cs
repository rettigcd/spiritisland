namespace SpiritIsland.NatureIncarnate;

public class SurroundedByTheDahan {

	public const string Name = "Surrounded by the Dahan";

	[SpiritCard(Name,0,Element.Moon,Element.Air,Element.Animal),Fast]
	[FromPresence(0,Filter.Dahan)]
	[Instructions( "2 Fear if Invaders are present. 1 Fear if Dahan outnumber Town/City. Isolate target land." ), Artist( Artists.AalaaYassin )]
	static public async Task ActionAsync(TargetSpaceCtx ctx) {
		// 2 Fear if Invaders are present.
		if(ctx.HasInvaders)
			await ctx.AddFear(2);
		// 1 Fear if Dahan outnumber Town/City.
		if(ctx.Space.SumAny(Human.Town_City) < ctx.Space.Sum(Human.Dahan))
			await ctx.AddFear(1);
		// Isolate target land.
		ctx.Isolate();
	}

}