namespace SpiritIsland.BranchAndClaw;

public class SpurOnWithWordsOfFire {

	[MinorCard("Spur on With Words of Fire", 1, Element.Sun, Element.Fire, Element.Air),Fast,AnySpirit]
	[Instructions( "If you target a Spirit other than yourself, they gain +1 Energy. Target Spirit may immediately play another Power Card by paying its cost. (If it is Slow, it does not resolve until later.)" ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpiritCtx ctx) {

		// If you target a spirit other than yourself, they gain +1 energy
		if(ctx.Other != ctx.Self)
			ctx.Other.Energy++;

		// target spirit may immediately play another power Card by paying its cost.
		// if it is slow, it does not resolve until later
		await ctx.Other.SelectAndPlayCardsFromHand(1);

	}

}