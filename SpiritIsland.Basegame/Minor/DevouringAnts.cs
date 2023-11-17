namespace SpiritIsland.Basegame;

public class DevouringAnts {
	public const string Name = "Devouring Ants";

	[MinorCard(Name,1,Element.Sun,Element.Earth,Element.Animal),Slow,FromSacredSite(1)]
	[Instructions( "1 Fear. 1 Damage. Destroy 1 Dahan. If target land is Jungle / Sands, +1 Damage." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// 1 fear
		ctx.AddFear(1);

		//  1 damage
		int damage = 1;

		// destroy 1 dahan
		if(ctx.Dahan.Any)
			await ctx.Dahan.Destroy(1);

		// if target is J / S, +1 damage
		if( ctx.IsOneOf( Terrain.Jungle, Terrain.Sands ) )
			++damage;

		await ctx.DamageInvaders( damage );
	}

}