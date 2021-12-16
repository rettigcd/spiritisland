using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Retreat : IFearOptions {

		public const string Name = "Retreat";

		[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
		public Task Level1( FearCtx ctx ) {
			return ForEachSpiritPushUpToNTokesnsFromInland( ctx, 2, Invader.Explorer );
		}

		[FearLevel( 2, "Each player may Push up to 3 Explorer / Town from an Inland land." )]
		public Task Level2( FearCtx ctx ) {
			return ForEachSpiritPushUpToNTokesnsFromInland( ctx, 3, Invader.Town, Invader.Explorer );
		}

		[FearLevel( 3, "Each player may Push any number of Explorer / Town from one land." )]
		public Task Level3( FearCtx ctx ) {
			return ForEachSpiritPushUpToNTokesnsFromInland( ctx, int.MaxValue, Invader.Town, Invader.Explorer );
		}

		static async Task ForEachSpiritPushUpToNTokesnsFromInland( FearCtx ctx, int max, params TokenGroup[] pushableInvaders ) {
			var gs = ctx.GameState;
			foreach(var spiritCtx in ctx.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => !s.IsCoastal && gs.Tokens[s].Has(Invader.Explorer) ).ToArray();
				if(options.Length == 0) break;
				var target = await spiritCtx.Decision( new Select.Space( $"Fear:select land to push up to {max} invaders", options, Present.Always ));
				await spiritCtx.Target(target).PushUpTo( max, pushableInvaders );
			}
		}

	}
}
