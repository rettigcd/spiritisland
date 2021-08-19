using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class LureOfTheUnknown {
		[MinorCard( "Lure of the Unknown", 0, Speed.Fast, Element.Moon, Element.Fire, Element.Air, Element.Plant )]
		[FromPresence( 2, Target.NoInvader )]
		public static Task ActAsync( TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			return ctx.GatherUpToNInvaders( target, 1, Invader.Explorer, Invader.Town );
		}
	}


}
