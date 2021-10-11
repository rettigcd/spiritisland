using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SuddenAmbush {

		public const string Name = "Sudden Ambush";

		[SpiritCard( SuddenAmbush.Name, 2, Element.Fire, Element.Air, Element.Animal )]
		[Fast]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {

			// you may gather 1 dahan
			await ctx.GatherUpToNDahan( 1 );

			// Each dahan destroys 1 explorer
			await ctx.Invaders.Destroy( ctx.DahanCount, Invader.Explorer );
		}

	}
}
