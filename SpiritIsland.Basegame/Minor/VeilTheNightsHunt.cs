using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( $"Each dahan deals 1 damage to a different invader", () => DamageDifferentInvaders(ctx, 1, ctx.DahanCount, Invader.City,Invader.Town,Invader.Explorer ) ),
				new ActionOption( "push up to 3 dahan", () => ctx.PushUpToNTokens( 3, TokenType.Dahan ) )
			);

		}

		static async Task DamageDifferentInvaders(TargetSpaceCtx ctx, int damagePerInvader, int numberToDamage, params TokenGroup[] groups ) {
			if(damagePerInvader<1) return;

			int damageableInvaderCount = ctx.Tokens.SumAny(groups);
			if( damageableInvaderCount <= numberToDamage) {
				await ctx.PowerInvaders.ApplyDamageToEach(damagePerInvader,groups);
				return;
			}

			var orig = new CountDictionary<Token>();
			foreach(var t in ctx.Tokens.OfAnyType( groups ))
				orig[t] = ctx.Tokens[t];

			// get invaders
			while(numberToDamage-- > 0) {
				var invader = await ctx.Self.Action.Decide(new SelectInvaderToDamage(damagePerInvader,ctx.Target,orig.Keys,Present.Done));
				if(invader != null) {
					await ctx.PowerInvaders.ApplyDamageTo1(damagePerInvader,invader);
					orig[invader]--;
				}
				else
					numberToDamage = 0;
			}

		}

	}
}
