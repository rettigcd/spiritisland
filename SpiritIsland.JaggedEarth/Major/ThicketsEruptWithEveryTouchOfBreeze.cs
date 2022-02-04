namespace SpiritIsland.JaggedEarth;

public class ThicketsEruptWithEveryTouchOfBreeze {

	[MajorCard("Thickets Erupt with Every Touch of Breeze",3,Element.Air,Element.Plant), Fast, FromSacredSite(2,Target.Inland)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 damage.
		await ctx.DamageInvaders( 2 );

		// Then either: add 3 wilds    OR    Remove 1 blight
		await ctx.SelectActionOption( Cmd.AddWilds(3), Cmd.RemoveBlight );

		// if you have 3 plant,
		if( await ctx.YouHave("3 plant" )) {
			// 1 fear
			ctx.AddFear(1);
			// +2 damage
			await ctx.DamageInvaders(2);
		}
	}

}