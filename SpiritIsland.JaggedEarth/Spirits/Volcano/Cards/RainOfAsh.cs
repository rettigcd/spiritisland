using System.Security.Cryptography;

namespace SpiritIsland.JaggedEarth;

public class RainOfAsh {

	[SpiritCard("Rain of Ash", 2, Element.Fire, Element.Air, Element.Earth), Slow, FromPresence(1)]
	[Instructions( "2 Fear if Invaders are present. Push 2 Dahan and 2 Explorer / Town to land(s) without your Presence." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) { 
		// 2 fear if Invaders are present.
		if(ctx.HasInvaders)
			await ctx.AddFear(2);

		// Push 2 dahan and 2 explorer / town to land(s) without your presence.
		await ctx.SourceSelector
			.AddGroup( 2, Human.Explorer_Town )
			.AddGroup( 2, Human.Dahan )
			.ConfigDestination(d=>d.FilterDestination( s => !ctx.Self.Presence.IsOn(s) ))
			.PushN(ctx.Self);
	}

}