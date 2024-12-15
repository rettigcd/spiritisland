namespace SpiritIsland.FeatherAndFlame;

public class GiftOfThePrimordialDeeps {

	[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, AnotherSpirit]
	[Instructions( "Target Spirit gains a Minor Power. Target Spirit chooses to either: Play it immediately by paying its cost. -or- Gain 1 Moon and 1 Earth." ), Artist( Artists.JorgeRamos )]
	public static async Task ActAsync( TargetSpiritCtx ctx ) {
		await TargetSpiritAction( ctx.Other );
	}

	static async Task TargetSpiritAction( Spirit other ) {
		// target spirit gains a minor power.
		var powerCard = (await other.Draw.Minor()).Selected;

		// Target spirit chooses to either:
		await Cmd.Pick1(
			new SpiritAction( "Play it immediately by paying its cost", spirit => spirit.PlayCard( powerCard ) )
				.OnlyExecuteIf( powerCard.Cost <= other.Energy ),
			new SpiritAction( "Gains 1 moon and 1 earth", spirit => spirit.Elements.Add( Element.Moon, Element.Earth ) ),
			SpiritAction.NoAction
		).ActAsync( other );
	}
}