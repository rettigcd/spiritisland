namespace SpiritIsland.BranchAndClaw;

public class InsatiableHungerOfTheSwarm {

	[MajorCard( "Insatiable Hunger of the Swarm", 4, Element.Air, Element.Plant, Element.Animal ), Fast, FromSacredSite( 2 )]
	[Instructions( "Add 1 Blight. Add 2 Beasts. Gather up to 2 Beasts. Each Beasts deal 1 Fear, 2 Damage to Invaders and 2 Damage to Dahan. Destroy 1 Beasts. -If you have- 2 Air, 4 Animal: Repeat this Power on an adjacent land." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		static async Task ApplyPowerOnTarget( TargetSpaceCtx ctx ) {
			// add 1 blight.
			await ctx.AddBlight(1);

			// Add 2 beasts
			var beasts = ctx.Beasts;
			await beasts.Add(2);

			// Gather up to 2 beasts
			await ctx.GatherUpTo( 2, Token.Beast );

			// each beast deals:
			// 1 fear
			ctx.AddFear( beasts.Count );
			// 2 damage to invaders
			await ctx.DamageInvaders( beasts.Count * 2 );
			// and 2 damage to dahan.
			await ctx.DamageDahan( beasts.Count );

			// Destroy 1 beast.
			await beasts.Destroy(1);
		}
		await ApplyPowerOnTarget( ctx );

		// if you have 2 air, 4 animal, repeat power on adjacent land.
		if(await ctx.YouHave("2 air,4 animal")) {
			var adjCtx = await ctx.SelectAdjacentLand("Apply power to adjacent land");
			await ApplyPowerOnTarget( adjCtx );
		}
	}

}