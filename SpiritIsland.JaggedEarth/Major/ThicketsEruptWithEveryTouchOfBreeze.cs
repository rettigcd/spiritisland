namespace SpiritIsland.JaggedEarth;

public class ThicketsEruptWithEveryTouchOfBreeze {

	[MajorCard("Thickets Erupt With Every Touch of Breeze",3,Element.Air,Element.Plant), Fast, FromSacredSite(2,Filter.Inland)]
	[Instructions( "2 Damage. Then either: Add 3 Wilds. -or- Remove 1 Blight. -If you have- 3 Plant: 1 Fear. +2 Damage." ), Artist( Artists.JorgeRamos )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 damage.
		await ctx.DamageInvaders( 2 );

		// Then either: add 3 wilds    OR    Remove 1 blight
		await ctx.SelectActionOption( Cmd.AddWilds(3), Cmd.RemoveBlight );

		// if you have 3 plant,
		if( await ctx.YouHave("3 plant" )) {
			// 1 fear
			await ctx.AddFear(1);
			// +2 damage
			await ctx.DamageInvaders(2);
		}
	}

}