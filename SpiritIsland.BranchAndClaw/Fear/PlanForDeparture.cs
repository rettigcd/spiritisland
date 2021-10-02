using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class PlanForDeparture : IFearOptions {
		public const string Name = "Plan for Departure";

		[FearLevel( 1, "Each player may gather 1 town into a costal land." )]
		public async Task Level1( FearCtx ctx ) {

			// Each player may gather 1 town into a costal land.
			var coastal = ctx.GameState.Island.AllSpaces.Where(x=>x.IsCoastal).ToArray();
			foreach(var spiritCtx in ctx.Spirits )
				await spiritCtx.GatherExplorerToOne(coastal,1,Invader.Town);

		}

		[FearLevel( 2, "Each player may gather 1 explroer / town into a costal land.  Defend 2 in all costal lands." )]
		public async Task Level2( FearCtx ctx ) {

			// Each player may gather 1 explroer / town into a costal land.
			var coastal = ctx.GameState.Island.AllSpaces.Where( x => x.IsCoastal ).ToArray();
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.GatherExplorerToOne( coastal, 1, Invader.Town );

			// Defend 2 in all costal lands.
			foreach(var land in coastal)
				ctx.GameState.Tokens[land].Defend.Count += 2;
		}

		[FearLevel( 3, "Each player may gather 2 explorer / town into a costal land.  Defend 4 in all costal lands" )]
		public async Task Level3( FearCtx ctx ) {

			// Each player may gather 2 explorer / town into a costal land.
			var coastal = ctx.GameState.Island.AllSpaces.Where( x => x.IsCoastal ).ToArray();
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.GatherExplorerToOne( coastal, 1, Invader.Town );

			// Defend 4 in all costal lands
			foreach(var land in coastal)
				ctx.GameState.Tokens[land].Defend.Count += 4;
		}

	}

}
