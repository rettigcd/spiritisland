using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.PowerCards {

	[PowerCard(PullBeneathTheHungryEarth.Name,1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
	public class PullBeneathTheHungryEarth : BaseAction {

		public const string Name = "Pull Beneath the Hungry Earth";

		readonly Spirit spirit;

		public PullBeneathTheHungryEarth(Spirit spirit,GameState gameState):base(gameState){
			this.spirit = spirit;
			// If target land has your presence, 1 fear and 1 damage
			// If target land is Sand or Water, 1 damage
			engine.decisions.Push(new TargetSpaceRangeFromPresence(spirit,1,SelfPresenceOrSandOrWater,SelectTarget));
		}

		bool SelfPresenceOrSandOrWater(Space space) => GeneratesDamageOnly(space) 
			|| GeneratesDamageAndFear(space);

		bool GeneratesDamageOnly(Space space) => space.Terrain.IsIn(Terrain.Sand,Terrain.Wetland);
		bool GeneratesDamageAndFear(Space space) => spirit.Presence.Contains(space);

		void SelectTarget(Space space, ActionEngine engine){
			if(GeneratesDamageOnly(space))
				; // +1 damage
			if(GeneratesDamageAndFear(space)){
				// +1 damage
				gameState.AddFear(1);
			}
		}


	}

	static public class ContainerExtensions{
		// shorter syntax:
		// space.Terrain.IsIn(Terrain.Wetland,Terrain.Sand)
		// vs.
		// new Terraion[]{Terrain.Wetland,Terrain.Sand}.Contains(space.Terrain);
		static public bool IsIn<T>(this T needle, params T[] haystack )
			=> haystack.Contains(needle);
	}

}
