namespace SpiritIsland.NatureIncarnate;

public class EntwineTheFatesOfAll {
	const string Name = "Entwine the Fates of All";

	[SpiritCard( Name, 1, Element.Moon, Element.Water, Element.Earth, Element.Plant ), Fast, AnySpirit]
	[Instructions( "In one of target Spirit's lands, Defend 2 per Presence (from all Spirits)." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpiritCtx ctx ) {
		Space space = await ctx.Self.SelectAlways(
			"Select space to defend 2/presence.", 
			ctx.Other.Presence.Lands	// Target-Spirit's lands, not Self's lands
		);
		int presenceCount = GameState.Current.Spirits.Sum( s => s.Presence.CountOn(space) ); // !!! 
		ctx.Self.Target(space).Defend(2 * presenceCount );
	}
}
