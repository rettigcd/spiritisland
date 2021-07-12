using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Minor {

	class DarkAndTangledWoods {

		[MinorCard("Dark and Tangled Woods", 1, Speed.Fast, Element.Moon, Element.Earth, Element.Plant)]
		static public async Task Act(ActionEngine eng){
			// range 1
			var target = await eng.TargetSpace_Presence(1);
			// 2 fear
			eng.GameState.AddFear(2);

			// if target is M/J, Defend 3
			if(target.Terrain.IsIn(Terrain.Jungle,Terrain.Mountain))
				eng.GameState.Defend(target,3);
		}

	}
}
