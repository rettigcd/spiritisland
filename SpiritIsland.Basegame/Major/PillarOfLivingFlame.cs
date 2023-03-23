namespace SpiritIsland.Basegame;

public class PillarOfLivingFlame {

	public const string Name = "Pillar of Living Flame";

	[MajorCard( Name, 5,Element.Fire),Slow,FromSacredSite(2)]
	[Instructions( "3 Fear. 5 Damage. If target land is Jungle / Wetland, add 1 Blight. -If you have- 4 Fire: +2 Fear and +5 Damage." ), Artist( Artists.JorgeRamos )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		int fear = 3;
		int damage = 5;

		// if you have 4 fire
		if( await ctx.YouHave("4 fire" )) {
			fear += 2;
			damage += 5;
		}

		// if target is Jungle / Wetland, add 1 blight
		if(ctx.IsOneOf( Terrain.Jungle, Terrain.Wetland ))
			await ctx.AddBlight(1);

		ctx.AddFear( fear );
		await ctx.DamageInvaders( damage );

	}

}