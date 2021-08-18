using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class ShatterHomesteads {
		public const string Name = "Shatter Homesteads";

		[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
		[FromSacredSite(2)]
		static public async Task Act(ActionEngine engine, Space target){
			// Destroy 1 town
			await engine.InvadersOn(target).Destroy( Invader.Town, 1 );

			// 1 fear
			engine.AddFear(1);
		}

	}

}
