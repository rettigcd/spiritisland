using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class AvoidTheDahan : IFearOptions {
		public const string Name = "Avoid the Dahan";

		[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
		public Task Level1( FearCtx ctx ) {

			ctx.GameState.PreExplore.ForRound.Add( ( gs, args ) => {
				for(int i = 0; i < args.SpacesMatchingCards.Count; ++i) {
					var space = args.SpacesMatchingCards[i];
					if( 2<=gs.DahanOn(space).Count)
						args.Skip(space);
				}
			} );

			return Task.CompletedTask;
		}

		[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
		public Task Level2( FearCtx ctx ) {
			ctx.GameState.PreBuilding.ForRound.Add( ( gs, args ) => {
				foreach(var space in args.SpaceCounts.Keys.ToArray()) {
					var tokens = gs.Tokens[space];
					if(tokens.SumAny(Invader.City,Invader.Town) < tokens.Dahan.Count)
						args.SpaceCounts[space] = 0;
				}
			} );

			return Task.CompletedTask;
		}

		[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
		public Task Level3( FearCtx ctx ) {
			ctx.GameState.PreBuilding.ForRound.Add( ( gs, args ) => {
				foreach(var space in args.SpaceCounts.Keys.ToArray()) {
					if(0 < gs.Tokens[space].Dahan.Count)
						args.SpaceCounts[space] = 0;
				}
			} );
			return Task.CompletedTask;
		}

	}

}

