namespace SpiritIsland.Basegame;

public class EncompassingWard {

	public const string Name = "Encompassing Ward";

	[MinorCard(EncompassingWard.Name,1,Element.Sun,Element.Water,Element.Earth),Fast,AnySpirit]
	[Instructions( "Defend 2 in every land where target Spirit has Presence." ), Artist( Artists.JorgeRamos )]
	static public Task Act( TargetSpiritCtx ctx ) {

		// defend 2 in every land where spirit has presence
		// defend should move with presence
		// https://querki.net/u/darker/spirit-island-faq/#!.7w4ganu
		GameState.Current.AddIslandMod( new DefendWherePresent(ctx.Other.Presence, 2, "🛡️") );

		return Task.CompletedTask;
	}

	public class DefendWherePresent( SpiritPresence presence, int amount, string badge ) : DefendToken(badge) {
		protected override int GetCount( Space space ) => 0 < space[presence.Token] ? amount : 0;
	}

}