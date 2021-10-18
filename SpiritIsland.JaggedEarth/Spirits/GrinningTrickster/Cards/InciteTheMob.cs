using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class InciteTheMob {

		[SpiritCard("Incite the Mob",1, Element.Moon,Element.Fire,Element.Air ), Slow, FromPresence(1,Target.Invaders)]
		static public async Task ActAsymc(TargetSpaceCtx ctx ) { 

			int startingInvaderCount = ctx.Tokens.InvaderTotal();

			// 1 invader with strife deals damage to other invaders (not to each)
			int damage = StrifedRavage.DamageFrom1StrifedInvaders( ctx.Tokens );
			await StrifedRavage.DamageUnStriffed(ctx,damage);

			// 1 fear per invader this power destroyed. // ??? What if Bringer uses this?  Does nightmare death count as death
			int killed = startingInvaderCount - ctx.Tokens.InvaderTotal();
			ctx.AddFear( killed );

		}

	}

}
