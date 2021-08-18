using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class IndomitableClaim {

		[MajorCard( "Indomitable Claim", 4, Speed.Fast, Element.Sun, Element.Earth )]
		[FromPresence( 1 )]
		static public async Task ActAsync( ActionEngine engine, Space target ) {
			var (self,gs) = engine;
			// add 1 presence in target land even if you normally could not due to land type.
			var source = await engine.Self.SelectTrack();
			self.Presence.PlaceFromBoard(source,target);
			// Defend 20
			gs.Defend(target,20);
			// if you have 2 sun, 3 earth,
			if(self.Elements.Contains("2 sun,3 earth" )) {
				// 3 fear
				engine.AddFear(3);
				// if invaders are present, Invaders skip all actions in target land this turn.
				if(gs.HasInvaders(target))
					gs.SkipAllInvaderActions(target);
			}
		}

	}
}
