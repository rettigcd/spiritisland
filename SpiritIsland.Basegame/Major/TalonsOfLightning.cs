namespace SpiritIsland.Basegame;

public class TalonsOfLightning {

	[MajorCard( "Talons of Lightning", 6, Element.Fire, Element.Air ),Fast,ExtendableRange( TargetFrom.Presence, 1, "3 fire,3 air", 2, Filter.Mountain, Filter.Wetland )]
	[Instructions( "3 Fear. 5 Damage. -If you have- 3 Fire, 3 Air: Destroy 1 Town in each adjacent land. Increase this power's Range to 3 Range." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 3 fear
		await ctx.AddFear(3);

		// 5 damage
		await ctx.DamageInvaders(5);

		if( await ctx.YouHave("3 fire,3 air" )){
			// destroy 1 town in each adjacent land
			foreach(var neighbor in ctx.Space.Adjacent)
				await ctx.Target(neighbor).Invaders.DestroyNOfClass( 1,Human.Town);

		}

	}

}