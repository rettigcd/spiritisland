namespace SpiritIsland.FeatherAndFlame;

public class ThreateningFlames {

	public const string BlightAndInvaders = "Blight+Invaders";

	[SpiritCard("Threatening Flames",0,Element.Fire,Element.Plant)]
	[Fast]
	[FromPresence(0,BlightAndInvaders)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 2 fear
		ctx.AddFear(2);

		bool HasNoPresence(SpaceState spaceState) => !ctx.Self.Presence.IsOn( spaceState );
		if( ctx.Adjacent.Any( HasNoPresence ) )
			// Push 1 explorer / town per Terror Level from target land to adjacent lands without your presence
			await ctx.Pusher
				.AddGroup(ctx.GameState.Fear.TerrorLevel, Human.Explorer_Town)
				.FilterDestinations( HasNoPresence )
				.MoveN();
		else
			// If there are no such adjacent lands, +2 fear
			ctx.AddFear(2);

	}

}