using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class ElementalAegis {

		[SpiritCard("Elemental Aegis",1,Element.Fire,Element.Water,Element.Earth),Fast]
		[FromPresence(0)]
		public static Task ActAsync(TargetSpaceCtx ctx ) {

			// defend 2 in target land and all adjacent lands.
			// For every presence on your Deep Slumber track, Defend 1 in target land and all adjacent lands.

			int defense = 2 // Defense 2 
				+ 0; // + covered Deep Slumber spaces // !!!

			ctx.Defend(defense);
			foreach(var adj in ctx.Adjacent )
				ctx.Target(adj).Defend(defense);

			return Task.CompletedTask;
		}
	}


}
