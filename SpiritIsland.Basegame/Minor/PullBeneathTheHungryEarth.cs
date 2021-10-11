using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PullBeneathTheHungryEarth {

		public const string Name = "Pull Beneath the Hungry Earth";

		[MinorCard(PullBeneathTheHungryEarth.Name,1,Element.Moon,Element.Water,Element.Earth)]
		[Slow]
		[FromPresence(1,Target.Any)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			int damage = 0;

			// If target land has your presence, 1 fear and 1 damage
			if( ctx.HasSelfPresence ){
				++damage;
				ctx.AddFear(1);
			}

			// If target land is Sand or Water, 1 damage
			if(ctx.IsOneOf( Terrain.Sand, Terrain.Wetland ))
				++damage;

			return ctx.DamageInvaders(damage);

		}

	}

}
