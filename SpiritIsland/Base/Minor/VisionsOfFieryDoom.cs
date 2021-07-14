using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class VisionsOfFieryDoom {
		[MinorCard("Visions of Fiery Doom",1, Speed.Fast,Element.Moon,Element.Fire)]
		static public async Task Act(ActionEngine eng){
			var target = await eng.Api.TargetSpace_Presence(0);
			// 1 fear
			bool hasBonus = eng.Self.Elements(Element.Fire)>=2;
			eng.GameState.AddFear(hasBonus?2:1);

			// Push 1 explorer/town
			await eng.PushUpToNInvaders(target,1,Invader.Explorer,Invader.Town);
		}
	}
}
