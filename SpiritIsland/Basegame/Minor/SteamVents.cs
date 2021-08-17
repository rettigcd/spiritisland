using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class SteamVents {


		[MinorCard("Steam Vents", 1, Speed.Fast, "fire,air,water,earth")]
		[FromPresence(0)]
		static public async Task ActAsync(ActionEngine eng,Space target ) {
			var grp = eng.InvadersOn(target);

			// if your have 3 earth, 
			if(3<=eng.Self.Elements[Element.Earth] && grp.HasTown)
				// instead destroy 1 town
				await grp.Destroy(Invader.Town,1);
			else
				// destroy 1 explorer
				await grp.Destroy(Invader.Explorer,1);
		}

	}
}
