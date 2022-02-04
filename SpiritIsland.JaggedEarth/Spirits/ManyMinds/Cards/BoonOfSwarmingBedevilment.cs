namespace SpiritIsland.JaggedEarth;

public class BoonOfSwarmingBedevilment {

	[SpiritCard("Boon of Swarming Bedevilment",0,Element.Air,Element.Water,Element.Animal), Fast, AnotherSpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		// for the rest of this turn, each of target Spirit's presence grants Defend 1 in its land.
		int PresenceAsToken(GameState _,Space space) => ctx.Self.Presence.CountOn(space);
		ctx.GameState.Tokens.RegisterDynamic( PresenceAsToken, TokenType.Defend, false );

		// Target Spirit may Push up to 1 of their presence.
		await ctx.OtherCtx.Presence.PushUpTo1();
	}

}