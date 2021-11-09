using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class RainOfAsh {
		[SpiritCard("Rain of Ash", 2, Element.Fire, Element.Air, Element.Earth), Slow, FromPresence(1)]
		public static Task ActAsync(TargetSpaceCtx _ ) { 
			// 2 fear if Invaders are present.
			// Push 2 dahan and 2 explorer / town to land(s) without your presence.
			return Task.CompletedTask;
		}
	}

}
