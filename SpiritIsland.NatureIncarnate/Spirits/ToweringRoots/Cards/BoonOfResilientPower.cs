namespace SpiritIsland.NatureIncarnate;

public class BoonOfResilientPower {

	const string Name = "Boon of Resilient Power";

	[SpiritCard( Name, 1, Element.Sun, Element.Moon, Element.Water, Element.Plant ), Slow, AnySpirit]
	[Instructions( "Target Spirit may Add 1 DestoryedPresence to one of your lands.  If you Target yourself, gain a Major Power, Otherwise, target Spirit gains a Power Card." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit may Add 1 DestoryedPresence to one of your lands.
		await new AddDestroyedPresence(0).RelativeTo(ctx.Self).ActAsync(ctx.OtherCtx);

		// If you Target yourself,
		if(ctx.Self == ctx.Other)
			// gain a Major Power,
			await ctx.Self.DrawMajor();
		// Otherwise,
		else
		// target Spirit gains a Power Card.
			await ctx.Other.Draw();
	}
}
