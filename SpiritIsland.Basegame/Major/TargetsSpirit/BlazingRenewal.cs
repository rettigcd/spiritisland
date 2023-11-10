using System;

namespace SpiritIsland.Basegame;

public class BlazingRenewal {

	[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant),Fast,AnySpirit]
	[Instructions( "Target Spirit adds 2 of their destroyed Presence into a single land, up to 2 Range from your Presence. If any Presence was added, 2 damage to each Town / City in that land. -If you have- 3 Fire, 3 Earth, 2 Plant: 4 Damage in that land." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		if(ctx.Other.Presence.Destroyed == 0) return;
		SelfCtx otherCtx = ctx.OtherCtx;

		Space space = await SelectTargetForRestoringDestoryedPresence( ctx, otherCtx );

		if(space != null)
			await OtherSpiritsAction(
				ctx.OtherCtx.Target( space ),
				// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
				await ctx.YouHave( "3 fire,3 earth,2 plant" )
			);

	}

	private static async Task<Space> SelectTargetForRestoringDestoryedPresence( TargetSpiritCtx ctx, SelfCtx otherCtx ) {
		// A Range extender effects the "Spirit's Actions".  (so Originating spirit's range determines which spaces)
		var targetOptions = ctx.Self.FindSpacesWithinRange( new TargetCriteria( 2 ), true )
			.Where( otherCtx.Self.Presence.CanBePlacedOn )  // filter by the OTHER spirits placeable options
			.ToArray();
		// Jonah says Originating Spirit's decision.  However, Querki says: Target Spirit makes the decision.
		Space space = (targetOptions.Length == 0) ? null
			: await ctx.OtherCtx.Decision( A.Space.ToPlacePresence( targetOptions, Present.Always, ctx.Other.Presence.Token ) );
		return space;
	}

	static async Task OtherSpiritsAction( TargetSpaceCtx ctx, bool escalate ) {

		// target spirit adds 2 of their destroyed presence
		await ctx.Presence.PlaceDestroyedHere( 2 );

		// if any presence was added,
		// 2 damage to each town/city in that land.

		// Do damage to city town
		await ctx.DamageEachInvader( 2, Human.Town_City );

		// Do escalation first to allow badland damage to be applied to anything
		if(escalate)
			await ctx.DamageInvaders( 4 );

		// This isn't 100% correct.
		// If escalation and Badlands, then should be able to apply badlands to explorers.
		// For simplifiation, just going to force it on the City/Towns if possible.

	}
}