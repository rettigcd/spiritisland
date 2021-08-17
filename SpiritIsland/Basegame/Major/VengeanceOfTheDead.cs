
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VengeanceOfTheDead {

		// 3 fast moon fire animal
		// range 3
		[MajorCard("Vengeance of the Dead",3,Speed.Fast,Element.Moon,Element.Fire,Element.Animal)]
		[FromPresence(3)]
		static public Task ActAsync(ActionEngine engine,Space target ) {
			// 3 fear
			engine.GameState.AddFear(3);

			var newDamageLands = new List<Space> { target };
			if(engine.Self.Elements.Contains("3 animal"))
				newDamageLands.AddRange(target.Adjacent.Where(x=>x.IsLand));

			async Task RavagePlusBonusDamage( RavageEngine eng ) {
				int damageInflictedFromInvaders = eng.GetDamageInflictedByInvaders();
				await eng.DamageLand( damageInflictedFromInvaders );
				int dahanKilled = await eng.DamageDahan( damageInflictedFromInvaders ); // !!! for consealing shadows remove this part
				int damageFromDahan = eng.GetDamageInflictedByDahan();
				var (cityKilled, townKilled, _) = await eng.DamageInvaders( damageFromDahan );

				// after each effect that destorys a town/city/dahan in target land
				// 1 damage per town/city/dahan destoryed
				await DistributeDamageToLands( engine, newDamageLands, dahanKilled + cityKilled + townKilled );
			}

			engine.GameState.ModRavage( target, cfg => cfg.RavageSequence = RavagePlusBonusDamage );

			return Task.CompletedTask;
		}

		static async Task DistributeDamageToLands( ActionEngine engine, List<Space> newDamageLands, int additionalDamage ) {
			Space[] targetLandOptions;
			while(additionalDamage > 0
				&& (targetLandOptions = newDamageLands.Where( engine.GameState.HasInvaders ).ToArray()).Length > 0
			) {
				var newLand = await engine.SelectSpace( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions );
				if(newLand == null) break;
				int damage = await engine.SelectNumber( "How many damage to apply?", additionalDamage );// !!! add include0 bool?
				await engine.DamageInvaders( newLand, damage );
				additionalDamage -= damage;
			}
		}
	}
}
