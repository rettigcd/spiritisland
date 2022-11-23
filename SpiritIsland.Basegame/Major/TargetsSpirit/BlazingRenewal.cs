namespace SpiritIsland.Basegame;

public class BlazingRenewal {

	[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant)]
	[Fast]
	[AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		if( ctx.Other.Presence.Destroyed == 0 ) return;

		// into a single land, up to range 2 from your presence.
		// Note - Jonah says it is the originators power and range and decision, not the targets
		var otherCtxPresence = ctx.OtherCtx.Presence;
		var spaceOptions = ctx.Self.GetTargetOptions(TargetingPowerType.None,ctx.GameState,new TargetSourceCriteria(From.Presence),new TargetCriteria(2,Target.Any))
			.Where( otherCtxPresence.CanBePlacedOn )
			.ToArray();
		TargetSpaceCtx selfPickLandCtx = await ctx.SelectSpace("Select location for target spirit to add presence", spaceOptions);

		// target spirit adds 2 of their destroyed presence
		await ctx.OtherCtx
			.Target( selfPickLandCtx.Space )
			.Presence.PlaceDestroyedHere(2);

		// if any presene was added, 2 damage to each town/city in that land.
		await selfPickLandCtx.DamageEachInvader(2,Invader.Town,Invader.City);

		// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
		if( await ctx.YouHave("3 fire,3 earth,2 plant") )
			await selfPickLandCtx.DamageInvaders(4);

	}

}