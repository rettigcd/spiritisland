namespace SpiritIsland.NatureIncarnate;

public class EntwineTheFatesOfAll {
	const string Name = "Entwine the Fates of All";

	[SpiritCard( Name, 1, Element.Moon, Element.Water, Element.Earth, Element.Plant ), Fast, AnySpirit]
	[Instructions( "In one of target Spirit's lands, Defend 2 per Presence (from all Spirits)." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpiritCtx ctx ) {
		var space = await ctx.Self.SelectAsync( new A.SpaceDecision( 
			"Select space to defend 2/presence.", 
			ctx.Other.Presence.Lands,	// Target-Spirit's lands, not Self's lands
			Present.Always 
		) );
		int presenceCount = GameState.Current.Spirits.Sum( s => s.Presence.CountOn(space) ); // !!! 
		space.Defend.Add(2 * presenceCount );
	}
}
