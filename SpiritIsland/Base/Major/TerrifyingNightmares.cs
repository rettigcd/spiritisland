using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class TerrifyingNightmares {

		[MajorCard("Terrifying Nightmares",4,Speed.Fast,Element.Moon,Element.Air)]
		static public async Task Act(ActionEngine eng){

			// range 2
			var target = await eng.Api.TargetSpace_Presence(2);

			// push up to 4 explorers or towns
			await eng.PushUpToNInvaders(target, 4, Invader.Explorer,Invader.Town);

			// 2 fear
			eng.GameState.AddFear(2);

			// if you have 4 moon, +4 fear
			if( 4<=eng.Self.Elements(Element.Moon) )
				eng.GameState.AddFear(4);



		}

	}

}
