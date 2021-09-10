
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

			var landsWeCanApplyTheDamageTo = new List<Space> { ctx.Space };

			// After each effect that destroys...
			async Task DealVengenceDamage( GameState gs, TokenDestroyedArgs args ) {
				//  ...a town / city / dahan in target land
				if( args.space == ctx.Space && args.Token.IsOneOf( Invader.Town, Invader.City, TokenType.Dahan) )
					// 1 damage per token destoryed
					await DistributeDamageToLands( ctx, landsWeCanApplyTheDamageTo, 1 );
			}
			ctx.GameState.Tokens.TokenDestroyed.ForRound.Add( DealVengenceDamage );

			// if you have 3 animal
			if(ctx.YouHave( "3 animal" ))
				// damage may be dealt into adjacent lands
				landsWeCanApplyTheDamageTo.AddRange( ctx.Adjacents );

			return Task.CompletedTask;
		}

		static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<Space> newDamageLands, int additionalDamage ) {
			Space[] targetLandOptions;
			while(additionalDamage > 0
				&& (targetLandOptions = newDamageLands.Where( ctx.GameState.HasInvaders ).ToArray()).Length > 0
			) {
				var newLand = await ctx.Self.Action.Decision( new Decision.TargetSpace( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions ));
				if(newLand == null) break;
				int damage = await ctx.Self.SelectNumber( "How many damage to apply?", additionalDamage, 0 );
				await ctx.DamageInvaders( newLand, damage );
				additionalDamage -= damage;
			}
		}
	}
}
