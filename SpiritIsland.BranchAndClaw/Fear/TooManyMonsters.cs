using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class TooManyMonsters : IFearOptions {
		public const string Name = "Too Many Monsters";

		[FearLevel( 1, "Each player removes 1 explorer / town from a land with beast." )]
		public async Task Level1( FearCtx ctx ) {

			// Each player removes 1 explorer / town from a land with beast.
			foreach(var spiritCtx in ctx.Spirits)
				await spiritCtx.RemoveTokenFromOneSpace( ctx.LandsWithBeasts(), 1, Invader.Explorer );

		}

		[FearLevel( 2, "Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a and adjacent to beast" )]
		public async Task Level2( FearCtx ctx ) {

			// Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast
			foreach(var spirit in ctx.Spirits)
				await RemoveTokenChoice( ctx,spirit, 1, Invader.Explorer );

		}

		[FearLevel( 3, "Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast" )]
		public async Task Level3( FearCtx ctx ) {

			// Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast
			foreach(var spirit in ctx.Spirits)
				await RemoveTokenChoice( ctx, spirit, 2, Invader.Explorer, Invader.Town );
		}

		static Task RemoveTokenChoice( FearCtx ctx, SelfCtx spiritCtx, int count, params TokenCategory[] interiorGroup ) {
			return spiritCtx.SelectActionOption(
				new SelfAction("Remove 1 explorer & 1 town from a land with beast", spiritCtx => { 
					return spiritCtx.RemoveTokenFromOneSpace( ctx.LandsWithBeasts(), count, Invader.Explorer, Invader.Town );
				}),
				new SelfAction("Remove 1 explorer from a land adjacent to beast", spiritCtx => { 
					return spiritCtx.RemoveTokenFromOneSpace( ctx.LandsAdjacentToBeasts(), 1, interiorGroup );
				})
			);
		}


	}

}
