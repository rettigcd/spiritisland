using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SuddenAmbush {

		public const string Name = "Sudden Ambush";

		[SpiritCard( SuddenAmbush.Name, 1, Speed.Slow, Element.Fire, Element.Air, Element.Animal )]
		[FromPresence(1)]
		static public async Task Act( ActionEngine engine, Space target ) {
			// you may gather 1 dahan
			await engine.GatherUpToNDahan(target, 1);

			// Each dahan destroys 1 explorer
			var grp = engine.GameState.InvadersOn(target);
			grp.Destroy(Invader.Explorer, engine.GameState.GetDahanOnSpace(target));
			grp.Commit();
		}

	}
}
