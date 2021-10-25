using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Element.Moon, Element.Air, Element.Animal)]
		[Fast]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( $"Each dahan deals 1 damage to a different invader", () => DamageDifferentInvaders(ctx, 1, ctx.Dahan.Count, Invader.City,Invader.Town,Invader.Explorer ) ),
				new ActionOption( "push up to 3 dahan", () => ctx.PushUpToNDahan( 3 ) )
			);

		}

		static async Task DamageDifferentInvaders(TargetSpaceCtx ctx, int damagePerInvader, int numberToDamage, params TokenGroup[] groups ) {
			if(damagePerInvader<1) return;

			int damageableInvaderCount = ctx.Tokens.SumAny(groups);
			if( damageableInvaderCount <= numberToDamage) {
				await ctx.DamageEachInvader(damagePerInvader,groups);
				return;
			}

			var orig = new CountDictionary<Token>();
			foreach(var t in ctx.Tokens.OfAnyType( groups ))
				orig[t] = ctx.Tokens[t];

			// get invaders
			while(numberToDamage-- > 0) {
				var invader = await ctx.Self.Action.Decision(new Decision.InvaderToDamage(damagePerInvader,ctx.Space,orig.Keys,Present.Done));
				if(invader != null) {
					await ctx.Invaders.ApplyDamageTo1(damagePerInvader,invader);
					orig[invader]--;
				}
				else
					numberToDamage = 0;
			}

		}

	}
}
