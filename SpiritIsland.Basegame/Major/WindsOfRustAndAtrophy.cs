namespace SpiritIsland.Basegame;

public class WindsOfRustAndAtrophy {

	[MajorCard("Winds of Rust and Atrophy",3,Element.Air,Element.Water,Element.Animal), Fast, FromSacredSite(3), RepeatIf("3 air,3 water,2 animal")]
	[Instructions( "1 Fear and Defend 6. Replace 1 City with 1 Town or 1 Town with 1 Explorer. -If you have- 3 Air, 3 Water, 2 Animal: Repeat this Power." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 1 fear and defend 6
		ctx.AddFear( 1 );
		ctx.Defend( 6 );

		// replace 1 city with 1 town OR 1 town with 1 explorer
		await ReplaceInvader.Downgrade( ctx, Present.Always, Human.Town_City );
	}

}