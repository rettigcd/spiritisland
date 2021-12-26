using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EmigrationAccelerates : IFearOptions {

		public const string Name = "Emigration Accelerates";
		string IFearOptions.Name => Name;

		[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( ctx, x => x.IsCoastal, Invader.Explorer );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a Coastal land." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( ctx, x => x.IsCoastal, Invader.Town, Invader.Explorer );
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachSpiritSelectedLandRemoveInvader( ctx, x => true, Invader.Town, Invader.Explorer );
		}

		static async Task ForEachSpiritSelectedLandRemoveInvader( 
			FearCtx ctx, 
			Func<Space, bool> landFilter, 
			params TokenClass[] removable
		) {
			foreach(var spirit in ctx.Spirits) {

				var options = spirit.AllSpaces
					.Where( landFilter )
					.Where( x => ctx.GameState.Tokens[x].HasAny( removable ) )
					.ToArray();

				await spirit.RemoveTokenFromOneSpace(options,1,removable);

			}
		}

	}

}

