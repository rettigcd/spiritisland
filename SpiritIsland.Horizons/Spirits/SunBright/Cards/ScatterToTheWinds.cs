namespace SpiritIsland.Horizons;

public class ScatterToTheWinds {

	public const string Name = "Scatter to the Winds";

	[SpiritCard(Name,1,Element.Fire,Element.Air,Element.Water),Slow,FromPresence(2)]
	[Instructions( "Choose up to 5 Explorer/Town/Dahan. Push them to as many different lands as possible." ), Artist( Artists.LucasDurham)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Choose up to 5 Explorer/Town/Dahan.
		// Push them to as many different lands as possible.
		return ctx.SourceSelector
			.AddGroup(5, Human.Explorer, Human.Town,Human.Dahan)
			.ConfigDestination(Distribute.ToAsManyLandsAsPossible)
			.PushN(ctx.Self);
	}

}
