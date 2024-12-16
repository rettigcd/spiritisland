namespace SpiritIsland.Basegame;

public class GuardTheHealingLand {

	[SpiritCard("Guard the Healing Land",3,Element.Water,Element.Earth,Element.Plant),Fast,FromSacredSite(1)]
	[Instructions( "Remove 1 Blight. Defend 4." ), Artist( Artists.SydniKruger )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// remove 1 blight
		await ctx.RemoveBlight();

		// defend 4
		ctx.Defend(4);

	}

}