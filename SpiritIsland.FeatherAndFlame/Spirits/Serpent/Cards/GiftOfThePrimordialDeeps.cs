namespace SpiritIsland.FeatherAndFlame;

public class GiftOfThePrimordialDeeps {

	[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, AnotherSpirit]
	[Instructions( "Target Spirit gains a Minor Power. Target Spirit chooses to either: Play it immediately by paying its cost. -or- Gain 1 Moon and 1 Earth." ), Artist( Artists.JorgeRamos )]
	public static async Task ActAsync( TargetSpiritCtx ctx ) {
		var otherCtx = ctx.OtherCtx;
		// target spirit gains a minor power.
		var powerCard = (await otherCtx.DrawMinor()).Selected;

		// Target spirit chooses to either:
		await otherCtx.SelectActionOption( 
			new SpiritAction( "Play it immediately by paying its cost", x => x.Self.PlayCard(powerCard) )
				.OnlyExecuteIf( powerCard.Cost <= otherCtx.Self.Energy ),
			new SpiritAction( "Gains 1 moon and 1 earth", _ => { var els = otherCtx.Self.Elements; els[Element.Moon]++; els[Element.Earth]++; }  )
		);

	}

}