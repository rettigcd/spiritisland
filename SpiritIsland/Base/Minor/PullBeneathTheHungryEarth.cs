using System;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[PowerCard(PullBeneathTheHungryEarth.Name,1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
	public class PullBeneathTheHungryEarth : TargetSpaceAction {

		public const string Name = "Pull Beneath the Hungry Earth";

		public PullBeneathTheHungryEarth(Spirit spirit,GameState gameState)
			:base(spirit,gameState,1,From.Presence)
		{}

		protected override bool FilterSpace(Space space) => GeneratesDamageOnly(space) 
			|| GeneratesDamageAndFear(space);

		protected override void SelectSpace(Space space){
			// If target land is Sand or Water, 1 damage
			if(GeneratesDamageOnly(space))
				; // +1 damage
			// If target land has your presence, 1 fear and 1 damage
			if(GeneratesDamageAndFear(space)){
				// +1 damage
				gameState.AddFear(1);
			}
		}

		bool GeneratesDamageOnly(Space space) => space.Terrain.IsIn(Terrain.Sand,Terrain.Wetland);
		bool GeneratesDamageAndFear(Space space) => self.Presence.Contains(space);


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
