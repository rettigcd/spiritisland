
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VengeanceOfTheDead {

		// 3 fast moon fire animal
		// range 3
		[MajorCard("Vengeance of the Dead",3,Speed.Fast,Element.Moon,Element.Fire,Element.Animal)]
		[FromPresence(3)]
		static public Task ActAsync(TargetSpaceCtx ctx) {
			// 3 fear
			ctx.AddFear(3);

			var landsWeCanApplyTheDamageTo = new List<Space> { ctx.Target };
			if(ctx.Self.Elements.Contains("3 animal"))
				landsWeCanApplyTheDamageTo.AddRange( ctx.PowerAdjacents() );

			async Task RavagePlusBonusDamage( RavageEngine ravageEngine ) {
				int damageInflictedFromInvaders = ravageEngine.GetDamageInflictedByInvaders();
				await ravageEngine.DamageLand( damageInflictedFromInvaders );
				int dahanKilled = await ravageEngine.DamageDahan( damageInflictedFromInvaders );
				int damageFromDahan = ravageEngine.GetDamageInflictedByDahan();

				var grpCounts = ravageEngine.Counts;
				int preCityCount = grpCounts.Sum(Invader.City);
				int preTownCount = grpCounts.Sum(Invader.Town);

				await ravageEngine.DamageInvaders( damageFromDahan );

				int cityKilled = preCityCount - grpCounts.Sum(Invader.City);
				int townKilled = preTownCount - grpCounts.Sum(Invader.Town);


				// after each effect that destorys a town/city/dahan in target land
				// 1 damage per town/city/dahan destoryed
				await DistributeDamageToLands( ctx, landsWeCanApplyTheDamageTo, dahanKilled + cityKilled + townKilled );
			}

			ctx.ModifyRavage( cfg => cfg.RavageSequence = RavagePlusBonusDamage );

			return Task.CompletedTask;
		}

		static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<Space> newDamageLands, int additionalDamage ) {
			Space[] targetLandOptions;
			while(additionalDamage > 0
				&& (targetLandOptions = newDamageLands.Where( ctx.GameState.HasInvaders ).ToArray()).Length > 0
			) {
				var newLand = await ctx.Self.Action.Decide( new TargetSpaceDecision( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions ));
				if(newLand == null) break;
				int damage = await ctx.Self.SelectNumber( "How many damage to apply?", additionalDamage, 0 );
				await ctx.DamageInvaders( newLand, damage );
				additionalDamage -= damage;
			}
		}
	}
}
