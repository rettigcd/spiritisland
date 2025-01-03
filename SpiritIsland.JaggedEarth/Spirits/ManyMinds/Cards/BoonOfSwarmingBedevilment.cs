namespace SpiritIsland.JaggedEarth;

public class BoonOfSwarmingBedevilment {

	[SpiritCard("Boon of Swarming Bedevilment",0,Element.Air,Element.Water,Element.Animal), Fast, AnotherSpirit]
	[Instructions( "For the rest of this turn, each of target Spirit's Presence grants Defend 1 in its land. Target Spirit may Push up to 1 of their Presence." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		// for the rest of this turn, each of target Spirit's presence grants Defend 1 in its land.
		DynamicToken.Defend(ctx.Self.Presence.CountOn, "🐝");

		// Target Spirit may Push up to 1 of their presence.
		await Cmd.PushUpTo1Presence().ActAsync( ctx.Other );
	}

}