namespace SpiritIsland.JaggedEarth;

public class InciteTheMob {

	[SpiritCard("Incite the Mob",1, Element.Moon,Element.Fire,Element.Air,Element.Animal ), Slow, FromPresence(1,Filter.Invaders)]
	[Instructions( "1 Invader with Strife deals Damage to other Invaders (not to each Invader). 1 Fear per Invader this Power Destroyed." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsymc(TargetSpaceCtx ctx ) { 

		int startingInvaderCount = ctx.Space.InvaderTotal();

		// 1 invader with strife deals damage to other invaders (not to each)
		var strifedInvaderWithMostDamage = ctx.Space.InvaderTokens().OfType<HumanToken>()
			.OrderByDescending( x => x.FullHealth )
			.FirstOrDefault();
		int damage = strifedInvaderWithMostDamage != null ? strifedInvaderWithMostDamage.FullHealth : 0;
		await ctx.StrifedDamageOtherInvaders(damage, strifedInvaderWithMostDamage, ctx.Space[strifedInvaderWithMostDamage] == 1 );

		// 1 fear per invader this power destroyed. // ??? What if Bringer uses this?  Does nightmare death count as death
		int killed = startingInvaderCount - ctx.Space.InvaderTotal();
		ctx.AddFear( killed );

	}

}