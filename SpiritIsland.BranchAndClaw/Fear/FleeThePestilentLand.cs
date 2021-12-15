using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class FleeThePestilentLand : IFearOptions {
		public const string Name = "Flee the Pestilent Land";

		[FearLevel( 1, "Each player removes 1 explorer/town from a land with disease" )]
		public async Task Level1( FearCtx ctx ) {

			// each player removes 1 explorer/town from a land with disease
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.RemoveTokenFromOneSpace( ctx.LandsWithDisease(), 1, Invader.Explorer, Invader.Town );
		}

		[FearLevel( 2, "Each player removes up to 3 health of invaders from a land with disease or 1 explorer from an inland land" )]
		public async Task Level2( FearCtx ctx ) {
			// each player removes up to 3 health of invaders from a land with disease or 1 explorer from an inland land
			foreach(var spiritCtx in ctx.Spirits)
				await RemoveHealthFromDiseaseOrExplorerInland( spiritCtx, 3, ctx.LandsWithDisease(), ctx.InlandLands );
		}

		[FearLevel( 3, "each player removes up to 5 health of invaders from a land with disease or 1 explorer/town from an inland land" )]
		public async Task Level3( FearCtx ctx ) {
			// each player removes up to 5 health of invaders from a land with disease or 1 explorer/town from an inland land
			foreach(var spiritCtx in ctx.Spirits)
				await RemoveHealthFromDiseaseOrExplorerInland( spiritCtx, 5, ctx.LandsWithDisease(), ctx.InlandLands );
		}

		static Task RemoveHealthFromDiseaseOrExplorerInland( SelfCtx spiritCtx, int healthToRemove, IEnumerable<Space> landsWithDisease, IEnumerable<Space> inlandSpaces ) {
			return spiritCtx.SelectActionOption(
				new SelfAction( $"Remove up to {healthToRemove} health of invaders from land with disease", spiritCtx => spiritCtx.RemoveHealthFromOne( healthToRemove, landsWithDisease ) ),
				new SelfAction( "Remove 1 explorer from inland", spiritCtx => spiritCtx.RemoveTokenFromOneSpace( inlandSpaces, healthToRemove, Invader.Explorer) )
			);
		}

	}

}
