namespace SpiritIsland.Basegame;

public class BlazingRenewal {

	[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant),Fast,AnySpirit]
	[Instructions( "Target Spirit adds 2 of their destroyed Presence into a single land, up to 2 Range from your Presence. If any Presence was added, 2 damage to each Town / City in that land. -If you have- 3 Fire, 3 Earth, 2 Plant: 4 Damage in that land." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// up to 2 Range from your Presence.
		await new AddDestroyedPresence(2).RelativeTo( ctx.Self )
			// adds 2 of their destroyed Presence into a single land,
			.SetNumToPlace( 2, Present.Always )
			// If any Presence was added,
			.WhenPlacedTrigger( async (count,space) => {
				var spaceCtx = ctx.Target(space);
				// 2 damage to each Town / City in that land.
				await spaceCtx.DamageEachInvader( 2, Human.Town_City );

				// If you have-3 Fire, 3 Earth, 2 Plant:
				if(await ctx.YouHave( "3 fire,3 earth,2 plant" ))
					// 4 Damage in that land.
					await spaceCtx.DamageInvaders( 4 );
			} )
			// Target Spirit
			.ActAsync( ctx.OtherCtx );
	}

}