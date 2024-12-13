namespace SpiritIsland.JaggedEarth;

public class BatsScoutForRaidsByDarkness{ 
		
	[MinorCard("Bats Scout for Raids by Darkness",1,Element.Moon,Element.Air,Element.Animal),Slow,FromPresence(2)]
	[Instructions( "For each Dahan, 1 Damage to Town / City. -or- 1 Fear. Gather up to 2 Dahan." ), Artist( Artists.ShawnDaley )]
	static public Task ActAsync(TargetSpaceCtx ctx){
		return ctx.SelectActionOption( 
			new SpaceAction("For each dahan, 1 Damage to town/city.", EachDahanDamagesTownOrCity ),
			new SpaceAction("1 Fear. Gather up to 2 Dahan", OneFearAndGatherUpTo2Dahan )
		);
	}

	static Task EachDahanDamagesTownOrCity(TargetSpaceCtx ctx ) => ctx.DamageInvaders(ctx.Dahan.CountAll,Human.Town_City);

	static async Task OneFearAndGatherUpTo2Dahan(TargetSpaceCtx ctx ) {
		await ctx.AddFear(1); 
		await ctx.GatherUpToNDahan(2);
	}

}