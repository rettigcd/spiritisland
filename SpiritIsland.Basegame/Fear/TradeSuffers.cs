using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearOptions {

		public const string Name = "Trade Suffers";

		[FearLevel( 1, "Invaders do not Build in lands with City." )]
		public Task Level1( FearCtx ctx ) {
			ctx.GameState.PreBuilding.ForRound.Add( ( gs, args ) => {
				foreach(var space in args.SpaceCounts.Keys.ToArray()) {
					if(0 < gs.Tokens[space].Sum(Invader.City))
						args.SpaceCounts[space] = 0;
				}
			} );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCoastal && gs.Tokens[s].Has( Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Select.Space( "Replace town with explorer", options, Present.Always ) );
				await ReplaceInvader.Downgrade( spirit, gs.Invaders.On( target, Cause.Fear ), Invader.Town );
			}
		}

		[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		public async Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => s.IsCoastal && gs.Tokens[ s ].HasAny(Invader.Town,Invader.City) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Select.Space( "Replace town with explorer or city with town", options, Present.Always ));
				await ReplaceInvader.Downgrade( spirit, gs.Invaders.On( target, Cause.Fear ), Invader.City, Invader.Town );
			}
		}

	}

}
