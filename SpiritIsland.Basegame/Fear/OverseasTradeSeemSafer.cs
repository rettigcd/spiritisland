using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class OverseasTradeSeemSafer : IFearOptions {

		public const string Name = "Overseas Trade Seem Safer";

		[FearLevel( 1, "Defend 3 in all Coastal lands." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			DefendCostal( gs, 3 );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			DefendCostal( gs, 6 );
			SkipCostalBuild( gs );
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			DefendCostal( gs, 9 );
			SkipCostalBuild( gs );
			return Task.CompletedTask;
		}

		static void DefendCostal( GameState gs, int defense ) {
			foreach(var space in gs.Island.AllSpaces.Where( s => s.IsCoastal ))
				gs.Tokens[space].Defend.Count += defense;
		}

		static void SkipCostalBuild( GameState gs ) {
			var spaces = gs.Island.AllSpaces.Where( s => s.IsCoastal ).ToArray();
			gs.PreBuilding.ForThisRound( ( GameState gs, BuildingEventArgs args ) => {
				foreach(var space in spaces)
					args.BuildTypes[space] = BuildingEventArgs.BuildType.TownsOnly; // !! This is not Additive, if something had Skip Towns, it might switch this to cities only
				return Task.CompletedTask;
			} );
		}

	}
}
