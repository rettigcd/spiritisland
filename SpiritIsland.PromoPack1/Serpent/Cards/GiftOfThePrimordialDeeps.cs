namespace SpiritIsland.PromoPack1;

public class GiftOfThePrimordialDeeps {

	[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, AnotherSpirit]
	public static async Task ActAsync( TargetSpiritCtx ctx ) {
		var otherCtx = ctx.OtherCtx;
		// target spirit gains a minor power.
		var powerCard = (await otherCtx.DrawMinor()).Selected;

		// Target spirit chooses to either:
		await otherCtx.SelectActionOption( 
			new SelfAction( "Play it immediately by paying its cost", x => x.Self.PlayCard(powerCard) )
				.FilterOption( powerCard.Cost <= otherCtx.Self.Energy ),
			new SelfAction( "Gains 1 moon and 1 earth", _ => { var els = otherCtx.Self.Elements; els[Element.Moon]++; els[Element.Earth]++; }  )
		);

	}

}