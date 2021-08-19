using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SuddenAmbush {

		public const string Name = "Sudden Ambush";

		[SpiritCard( SuddenAmbush.Name, 1, Speed.Fast, Element.Fire, Element.Air, Element.Animal )]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			// you may gather 1 dahan
			await ctx.GatherUpToNDahan(target, 1);

			// Each dahan destroys 1 explorer
			var grp = ctx.InvadersOn(target);
			int dahahCount = ctx.GameState.GetDahanOnSpace( target );
			await grp.Destroy(Invader.Explorer, dahahCount);
		}

	}
}
