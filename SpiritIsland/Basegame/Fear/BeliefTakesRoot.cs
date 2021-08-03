using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class BeliefTakesRoot : IFearCard {

		[FearLevel( 1, "Defend 2 in all lands with Presence." )]
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Spirits.SelectMany( s => s.Presence ).Distinct())
				gs.Defend( space, 2 );
			return Task.CompletedTask;
		}

		//	"Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders.", 
		[FearLevel( 2, "" )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//	"Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there."),
		[FearLevel( 3, "" )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}

