
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VengeanceOfTheDead {

		[MajorCard("Vengeance of the Dead",3,Element.Moon,Element.Fire,Element.Animal), Fast, FromPresence(3)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {
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
			ctx.GameState.Tokens.TokenDestroyed.ForThisRound( DealVengenceDamage );

			// if you have 3 animal
			if(await ctx.YouHave( "3 animal" ))
				// damage may be dealt into adjacent lands
				landsWeCanApplyTheDamageTo.AddRange( ctx.Adjacent );

		}

		static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<Space> newDamageLands, int additionalDamage ) {
			Space[] targetLandOptions;
			while(additionalDamage > 0
				&& (targetLandOptions = newDamageLands.Where( s => ctx.Target(s).HasInvaders ).ToArray()).Length > 0
			) {
				var newLand = await ctx.Self.Action.Decision( new Decision.TargetSpace( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions, Present.Always ));
				if(newLand == null) break;
				int damage = await ctx.Self.SelectNumber( "How many damage to apply?", additionalDamage, 0 );
				await ctx.Target( newLand ).DamageInvaders( damage );
				additionalDamage -= damage;
			}
		}
	}
}
