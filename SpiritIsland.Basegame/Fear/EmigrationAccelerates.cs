using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EmigrationAccelerates : IFearOptions {

		public const string Name = "Emigration Accelerates";

		[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => x.IsCoastal, Invader.Explorer );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a Coastal land." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => x.IsCoastal, Invader.Town, Invader.Explorer );
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( gs, x => true, Invader.Town, Invader.Explorer );
		}

		static async Task ForEachSpiritSelectedLandRemoveInvader( 
			GameState gs, 
			Func<Space, bool> landFilter, 
			params TokenGroup[] removable
		) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( landFilter )
					.Where( x => gs.Tokens[x].HasAny( removable ) )
					.ToArray();
				if(options.Length == 0) break;
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Fear:Pick costal land remove invader", options, Present.Always ) );
				var grp = gs.Tokens[target];
				RemoveInvader( grp, removable );
			}
		}

		private static void RemoveInvader( TokenCountDictionary grp, TokenGroup[] removable ) {
			var invaderToRemove = grp.PickBestInvaderToRemove( removable );
			grp.Adjust( invaderToRemove, -1 );
		}
	}

}

