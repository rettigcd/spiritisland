namespace SpiritIsland.Basegame {

	public class PullBeneathTheHungryEarth {

		public const string Name = "Pull Beneath the Hungry Earth";

		[MinorCard(PullBeneathTheHungryEarth.Name,1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
		[FromPresence(1,Target.Any)] // !!! Add unit test that we don't accidentally switch this back to .SandOrWetland
		static public void ActAsync(ActionEngine eng, Space target){

			var (self,gameState) = eng;
			int damage = 0; // accumulate because +2 is better than +1 +1

			// If target land is Sand or Water, 1 damage
			if(GeneratesDamageOnly(target))
				++damage;

			// If target land has your presence, 1 fear and 1 damage
			if(self.Presence.IsOn( target)){
				++damage;
				gameState.AddFear(1);
			}

			gameState.DamageInvaders(target,damage);

		}

		static bool GeneratesDamageOnly(Space space) => space.Terrain.IsIn(Terrain.Sand,Terrain.Wetland);


	}

}
