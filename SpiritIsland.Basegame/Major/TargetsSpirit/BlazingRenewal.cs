namespace SpiritIsland.Basegame;

public class BlazingRenewal {

	[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant)]
	[Fast]
	[AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		if( ctx.Other.Presence.Destroyed == 0 ) return;

		// into a single land, up to range 2 from your presence.
		// Jonah says: it is the originators power and range AND DECISION, not the targets
		// Querki says: Target makes the decision
		// Jonah's version uses .SelectDestinationWithinRange which binds the source presence to the decision maker so this is simpler.

		var space = await ctx.Presence.SelectDestinationWithinRange(2,Target.Any,TargetingPowerType.PowerCard);

		// target spirit adds 2 of their destroyed presence
		await ctx.OtherCtx
			.Target( space )
			.Presence.PlaceDestroyedHere(2);

		// if any presene was added, 2 damage to each town/city in that land.
		var selfPickLandCtx = ctx.Target( space );
		await selfPickLandCtx.DamageEachInvader(2,Invader.Town,Invader.City);

		// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
		if( await ctx.YouHave("3 fire,3 earth,2 plant") )
			await selfPickLandCtx.DamageInvaders(4);

	}

}