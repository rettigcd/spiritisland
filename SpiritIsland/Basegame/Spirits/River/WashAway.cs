using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class WashAway {

		public const string Name = "Wash Away";

		[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
		[FromPresence(1,Target.TownOrExplorer)]
		static public async Task ActionAsync(ActionEngine engine,Space target){

			await engine.PushUpToNInvaders(target,3, Invader.Town,Invader.Explorer);

		}

	}

}
