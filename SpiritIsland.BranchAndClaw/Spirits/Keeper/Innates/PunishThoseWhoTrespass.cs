using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower( "Punish Those Who Trespass" ),Slow]
	[FromPresence( 0 )]
	public class PunishThoseWhoTrespass {

		[InnateOption("2 sun,1 fire,2 plant","2 Damage. Destroy 1 dahan.")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			// 2 damage.
			int damage = 2;
			return ActAsync( ctx, damage );
		}

		[InnateOption( "2 sun,2 fire,3 plant", "+1 Damage per sunplant you have." )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			// +1 damage per sunplant you have
			int damage = 2 + Math.Min( ctx.Self.Elements[Element.Sun], ctx.Self.Elements[Element.Plant] );
			return ActAsync( ctx, damage );
		}

		static async Task ActAsync( TargetSpaceCtx ctx, int damage ) {
			// Destroy 1 dahan
			await ctx.DestroyDahan( 1 );

			// 4 plant  split this power's damage however desired between target land and another 1 of your lands
			int damageToTarget = ctx.Self.Elements[Element.Air] < 4 && ctx.Self.Presence.Spaces.Count()>1
				? damage
				: await ctx.Self.SelectNumber("Damage to apply to "+ctx.Space.Label, damage );

			await ctx.DamageInvaders( damage );

			int remainingDamage = damage - damageToTarget;
			if(remainingDamage > 0) {
				var secondaryTarget = await ctx.Self.Action.Decision(new Decision.TargetSpace(
					$"Apply {remainingDamage} reamaining damage"
					,ctx.Self.Presence.Spaces
					, Present.Always
				));
				await ctx.Target(secondaryTarget).DamageInvaders(remainingDamage);
			}

		}

		[InnateOption( "4 plant", "Split this Power's Damage however desired between target land and another 1 of your lands.", AttributePurpose.DisplayOnly )]
		static public void NoOp() {}


	}


}
