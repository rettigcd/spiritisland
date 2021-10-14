using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class DepartTheDangerousLand : IFearOptions {

		public const string Name = "Depart the Dangerous Land";

		[FearLevel( 1, "Each player removes 1 explorer from a land with beast, disease or at least 2 dahan" )]
		public async Task Level1( FearCtx ctx ) {

			// each player removes 1 explorer from a land with beast, disease or at least 2 dahan
			var spaceOptions = LandsWithBeastDiseaseOr2Dahan( ctx );
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.RemoveTokenFromOne( spaceOptions, 1, Invader.Explorer );
		}

		[FearLevel( 2, "Each player removes 1 explorer/town from a land with beast, disease or at least 2 dahan" )]
		public async Task Level2( FearCtx ctx ) {

			// Each player removes 1 explorer/town from a land with beast, disease or at least 2 dahan
			var spaceOptions = LandsWithBeastDiseaseOr2Dahan( ctx );
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.RemoveTokenFromOne( spaceOptions, 1, Invader.Explorer, Invader.Town );

		}

		[FearLevel( 3, "Each player removes up to 4 health worth of invaders from a land with beast, disease or at least 2 dahan" )]
		public async Task Level3( FearCtx ctx ) {

			// Each player removes up to 4 health worth of invaders from a land with beast, disease or at least 2 dahan
			var options = LandsWithBeastDiseaseOr2Dahan( ctx );
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.RemoveHealthFromOne( 4, options );
		}

		static Space[] LandsWithBeastDiseaseOr2Dahan( FearCtx ctx ) {
			return ctx.GameState.Island.AllSpaces
				.Where( s => {
					var tokens = ctx.GameState.Tokens[s];
					return tokens.Beasts.Count>0
						|| tokens.Disease.Count>0
						|| 2 <= tokens.Sum(TokenType.Dahan);
				} )
				.ToArray();
		}

	}

}
