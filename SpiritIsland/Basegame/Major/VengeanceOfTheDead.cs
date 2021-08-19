
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

			var newDamageLands = new List<Space> { ctx.Target };
			if(ctx.Self.Elements.Contains("3 animal"))
				newDamageLands.AddRange( ctx.Target.Adjacent.Where(x=>x.IsLand));

			async Task RavagePlusBonusDamage( RavageEngine eng ) {
				int damageInflictedFromInvaders = eng.GetDamageInflictedByInvaders();
				await eng.DamageLand( damageInflictedFromInvaders );
				int dahanKilled = await eng.DamageDahan( damageInflictedFromInvaders ); // !!! for consealing shadows remove this part
				int damageFromDahan = eng.GetDamageInflictedByDahan();
				var (cityKilled, townKilled, _) = await eng.DamageInvaders( damageFromDahan );

				// after each effect that destorys a town/city/dahan in target land
				// 1 damage per town/city/dahan destoryed
				await DistributeDamageToLands( ctx, newDamageLands, dahanKilled + cityKilled + townKilled );
			}

			ctx.ModRavage( cfg => cfg.RavageSequence = RavagePlusBonusDamage );

			return Task.CompletedTask;
		}

		static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<Space> newDamageLands, int additionalDamage ) {
			Space[] targetLandOptions;
			while(additionalDamage > 0
				&& (targetLandOptions = newDamageLands.Where( ctx.GameState.HasInvaders ).ToArray()).Length > 0
			) {
				var newLand = await ctx.Self.SelectSpace( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions );
				if(newLand == null) break;
				int damage = await ctx.Self.SelectNumber( "How many damage to apply?", additionalDamage );// !!! add include0 bool?
				await ctx.DamageInvaders( newLand, damage );
				additionalDamage -= damage;
			}
		}
	}
}
