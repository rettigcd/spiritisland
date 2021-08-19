using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class IndomitableClaim {

		[MajorCard( "Indomitable Claim", 4, Speed.Fast, Element.Sun, Element.Earth )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			var (self,gs) = ctx;
			// add 1 presence in target land even if you normally could not due to land type.
			var source = await ctx.Self.SelectTrack();
			self.Presence.PlaceFromBoard(source,ctx.Target);
			// Defend 20
			gs.Defend(ctx.Target,20);
			// if you have 2 sun, 3 earth,
			if(self.Elements.Contains("2 sun,3 earth" )) {
				// 3 fear
				ctx.AddFear(3);
				// if invaders are present, Invaders skip all actions in target land this turn.
				if(gs.HasInvaders(ctx.Target))
					gs.SkipAllInvaderActions( ctx.Target );
			}
		}

	}
}
