using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MinorCard(PullBeneathTheHungryEarth.Name,1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
	public class PullBeneathTheHungryEarth : TargetSpaceAction {

		public const string Name = "Pull Beneath the Hungry Earth";

		public PullBeneathTheHungryEarth(Spirit spirit,GameState gameState)
			:base(spirit,gameState,1,From.Presence)
		{}

		protected override bool FilterSpace(Space space) => GeneratesDamageOnly(space) 
			|| GeneratesDamageAndFear(space);

		protected override void SelectSpace(Space space){
			int damage = 0; // accumulate because +2 is better than +1 +1
			// If target land is Sand or Water, 1 damage
			if(GeneratesDamageOnly(space))
				++damage;
			// If target land has your presence, 1 fear and 1 damage
			if(GeneratesDamageAndFear(space)){
				++damage;
				gameState.AddFear(1);
			}
			if(damage>0)
				gameState.DamageInvaders(space,damage);
		}

		static bool GeneratesDamageOnly(Space space) => space.Terrain.IsIn(Terrain.Sand,Terrain.Wetland);
		bool GeneratesDamageAndFear(Space space) => self.Presence.Contains(space);


	}

}
