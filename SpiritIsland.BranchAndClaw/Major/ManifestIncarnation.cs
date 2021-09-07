using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ManifestIncarnation {

		static ManifestIncarnation() {
			SpaceFilter.lookup["Cities"] = (ctx, space) => ctx.GameState.Tokens[space].Has(Invader.City);
		}


		[MajorCard( "Manifest Incarnation", 6, Speed.Slow, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
		[FromPresence( 0, "Cities" )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// +1 fear for each town/city and for each of your presence in target land.  Remove 1 city, 1 town and 1 explorer.  Then, Invaders in target land ravage.
			// if you have 3 sun and 3 moon, invaders do -6 damage on their ravage.
			return Task.CompletedTask;
		}

	}
}
