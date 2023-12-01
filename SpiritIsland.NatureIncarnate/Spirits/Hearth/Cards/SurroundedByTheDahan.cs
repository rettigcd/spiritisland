namespace SpiritIsland.NatureIncarnate;

public class SurroundedByTheDahan {

	public const string Name = "Surrounded by the Dahan";

	[SpiritCard(SurroundedByTheDahan.Name,0,Element.Moon,Element.Air,Element.Animal),Fast]
	[FromPresence(0,Filter.Dahan)]
	[Instructions( "2 Fear if Invaders are present. 1 Fear if Dahan outnumber Town/City. Isolate target land." ), Artist( Artists.AalaaYassin )]
	static public Task ActionAsync(TargetSpaceCtx ctx) {
		// 2 Fear if Invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(2);
		// 1 Fear if Dahan outnumber Town/City.
		if(ctx.Tokens.SumAny(Human.Town_City) < ctx.Tokens.Sum(Human.Dahan))
			ctx.AddFear(1);
		// Isolate target land.
		ctx.Isolate();
		return Task.CompletedTask;
	}

}