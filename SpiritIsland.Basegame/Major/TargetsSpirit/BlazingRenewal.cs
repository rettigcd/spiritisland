namespace SpiritIsland.Basegame;

public class BlazingRenewal {

	[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant)]
	[Fast]
	[AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		if( ctx.Other.Presence.Destroyed == 0 ) return;

		// target spirit adds 2 of their destroyed presence
		// into a single land, up to range 2 from your presence.

		// A Range extender effects the "Spirit's Actions".  (so Originating spirit's range determines which spaces)
		var targetOptions = ctx.Presence.FindSpacesWithinRange( new TargetCriteria(2),TargetingPowerType.PowerCard);
		// Jonah says Originating Spirit's decision.  However, Querki says: Target Spirit makes the decision.
		var space = await ctx.OtherCtx.Decision( Select.Space.ToPlacePresence( targetOptions, Present.Always ) );

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