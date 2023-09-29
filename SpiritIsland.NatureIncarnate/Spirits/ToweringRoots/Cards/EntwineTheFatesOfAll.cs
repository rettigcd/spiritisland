namespace SpiritIsland.NatureIncarnate;

public class EntwineTheFatesOfAll {
	const string Name = "Entwine the Fates of All";

	[SpiritCard( Name, 1, Element.Moon, Element.Water, Element.Earth, Element.Plant ), Fast, AnySpirit]
	[Instructions( "In one of target Spirit's lands, Defend 2 per Presence (from all Spirits)." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpiritCtx ctx ) {
		Space space = await ctx.Other.SelectDeployedPresence("Select space to defend 2/presence.");
		SpaceState tokens = space.Tokens;
		tokens.Defend.Add(2 * tokens.Sum(TokenCategory.Presence) );
	}
}
