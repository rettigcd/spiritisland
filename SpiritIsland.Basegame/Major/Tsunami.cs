namespace SpiritIsland.Basegame;

public class Tsunami {

	public const string Name = "Tsunami";

	[MajorCard(Tsunami.Name,6,Element.Water,Element.Earth)]
	[Slow]
	[FromSacredSite(2,Target.Coastal)]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 2 fear
		ctx.AddFear(2);

		// +8 damage
		await ctx.DamageInvaders(8);

		// destroy 2 dahan
		await ctx.Dahan.Destroy( 2 );

		if(await ctx.YouHave("3 water,2 earth")){
			var otherCoastalsOnSameBoard = ctx.Space.Boards
				.SelectMany(b=>b.Spaces.Where( s=> s != ctx.Space && ctx.Target(s).IsCoastal ) )
				.Distinct()
				.ToArray();

			foreach(var otherCoast in otherCoastalsOnSameBoard)
				await DamageOtherCoast( ctx.Target( otherCoast ) );

		}
	}

	static async Task DamageOtherCoast( TargetSpaceCtx otherCtx ) {
		// 1 fear
		otherCtx.AddFear( 1 );
		// 4 damage
		await otherCtx.DamageInvaders( 4 );
		// destroy 1 dahan
		if(otherCtx.Dahan.Any)
			await otherCtx.Dahan.Destroy( 1 );
	}

}