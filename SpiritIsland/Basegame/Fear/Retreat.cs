using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Retreat : IFearCard {

		public const string Name = "Retreat";

		[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
		public Task Level1( GameState gs ) {
			return ForEachSpiritPushUpToNInvadersFromInland( gs, 2, Invader.Explorer );
		}

		[FearLevel( 2, "Each player may Push up to 3 Explorer / Town from an Inland land." )]
		public Task Level2( GameState gs ) {
			return ForEachSpiritPushUpToNInvadersFromInland( gs, 3, Invader.Town, Invader.Explorer );
		}

		[FearLevel( 3, "Each player may Push any number of Explorer / Town from one land." )]
		public Task Level3( GameState gs ) {
			return ForEachSpiritPushUpToNInvadersFromInland( gs, int.MaxValue, Invader.Town, Invader.Explorer );
		}

		static async Task ForEachSpiritPushUpToNInvadersFromInland( GameState gs, int max, params Invader[] pushableInvaders ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => !s.IsCostal && gs.Invaders.Counts[s].Has(Invader.Explorer) ).ToArray();
				if(options.Length == 0) break;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( $"Fear:select land to push up to {max} invaders", options ));
				await spirit.MakeDecisionsFor( gs ).FearPushUpToNInvaders( target, max, pushableInvaders );
			}
		}

	}
}
