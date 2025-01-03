namespace SpiritIsland.Basegame;

public class EncompassingWard {

	public const string Name = "Encompassing Ward";

	[MinorCard(EncompassingWard.Name,1,Element.Sun,Element.Water,Element.Earth),Fast,AnySpirit]
	[Instructions( "Defend 2 in every land where target Spirit has Presence." ), Artist( Artists.JorgeRamos )]
	static public Task Act( TargetSpiritCtx ctx ) {

		// defend 2 in every land where spirit has presence
		// defend should move with presence
		// https://querki.net/u/darker/spirit-island-faq/#!.7w4ganu
		DynamicToken.Defend(space => 0 < space[ctx.Other.Presence.Token] ? 2 : 0, "🛡️");

		return Task.CompletedTask;
	}

}