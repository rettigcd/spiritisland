using System.Security.Cryptography;

namespace SpiritIsland.FeatherAndFlame;

public class ThreateningFlames {

	[SpiritCard("Threatening Flames",0,Element.Fire,Element.Plant),Fast,FromPresence(0,Filter.BlightAndInvaders)]
	[Instructions( "2 Fear. Push 1 Explorer / Town per Terror Level from target land to adjacent lands without your Presence. If there are no such adjacent lands, +2 Fear." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 2 fear
		ctx.AddFear(2);

		bool HasNoPresence(Space space) => !ctx.Self.Presence.IsOn(space);
		if( ctx.Adjacent.Any( HasNoPresence ) )
			// Push 1 explorer / town per Terror Level from target land to adjacent lands without your presence
			await ctx.SourceSelector
				.AddGroup(GameState.Current.Fear.TerrorLevel, Human.Explorer_Town)
				.ConfigDestination( d=>d.FilterDestination( HasNoPresence ) )
				.PushN(ctx.Self);
		else
			// If there are no such adjacent lands, +2 fear
			ctx.AddFear(2);

	}

}