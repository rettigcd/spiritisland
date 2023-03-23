namespace SpiritIsland.JaggedEarth;

public class BoonOfSwarmingBedevilment {

	[SpiritCard("Boon of Swarming Bedevilment",0,Element.Air,Element.Water,Element.Animal), Fast, AnotherSpirit]
	[Instructions( "For the rest of this turn, each of target Spirit's Presence grants Defend 1 in its land. Target Spirit may Push up to 1 of their Presence." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		// for the rest of this turn, each of target Spirit's presence grants Defend 1 in its land.
		ctx.GameState.Tokens.Dynamic.ForRound.Register( ctx.Self.Presence.CountOn, Token.Defend );

		// Target Spirit may Push up to 1 of their presence.
		await ctx.Other.PushUpTo1Presence();
	}

}