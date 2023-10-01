namespace SpiritIsland.JaggedEarth;

public class RainOfAsh {

	[SpiritCard("Rain of Ash", 2, Element.Fire, Element.Air, Element.Earth), Slow, FromPresence(1)]
	[Instructions( "2 Fear if Invaders are present. Push 2 Dahan and 2 Explorer / Town to land(s) without your Presence." ), Artist( Artists.MoroRogers )]
	public static Task ActAsync(TargetSpaceCtx ctx ) { 
		// 2 fear if Invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(2);

		// Push 2 dahan and 2 explorer / town to land(s) without your presence.
		return ctx.Pusher
			.AddGroup( 2, Human.Explorer_Town )
			.AddGroup( 2, Human.Dahan )
			.FilterDestinations( s => !ctx.Self.Presence.IsOn(s) )
			.MoveN();
	}

}