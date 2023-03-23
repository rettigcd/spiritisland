namespace SpiritIsland.JaggedEarth;
public class ScarredAndStonyLand {

	[SpiritCard("Scarred and Stony Land",2,Element.Moon, Element.Earth), Slow, FromSacredSite(1,Target.Blight)]
	[Instructions( "2 Damage. Add 1 Badlands. Remove 1 Blight in target land from the game. (It goes to the box, not the Blight Card.)" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 2 damage
		await ctx.DamageInvaders( 2 );

		// Add 1 badlands
		await ctx.Badlands.Add(1);

		// Remove 1 blight in target land from the game. (It goes to the box, not the blight card)
		await ctx.Blight.Remove(1, RemoveReason.Removed); // it does not go back to card
	}

}