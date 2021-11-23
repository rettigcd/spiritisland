using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class RainOfAsh {

		[SpiritCard("Rain of Ash", 2, Element.Fire, Element.Air, Element.Earth), Slow, FromPresence(1)]
		public static Task ActAsync(TargetSpaceCtx ctx ) { 
			// 2 fear if Invaders are present.
			if(ctx.HasInvaders)
				ctx.AddFear(2);

			// Push 2 dahan and 2 explorer / town to land(s) without your presence.
			return ctx.Pusher
				.AddGroup( 2, Invader.Explorer, Invader.Town )
				.AddGroup( 2, TokenType.Dahan )
				.FilterDestinations( s => !ctx.Self.Presence.IsOn(s) )
				.MoveN();
		}
	}

}
