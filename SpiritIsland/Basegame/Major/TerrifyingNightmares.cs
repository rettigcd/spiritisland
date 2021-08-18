using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TerrifyingNightmares {

		[MajorCard("Terrifying Nightmares",4,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(ActionEngine eng,Space target){

			// push up to 4 explorers or towns
			await eng.PushUpToNInvaders(target, 4, Invader.Explorer,Invader.Town);

			// 2 fear
			eng.AddFear(2);

			// if you have 4 moon, +4 fear
			if( 4<=eng.Self.Elements[Element.Moon] )
				eng.AddFear(4);

		}

	}

}
