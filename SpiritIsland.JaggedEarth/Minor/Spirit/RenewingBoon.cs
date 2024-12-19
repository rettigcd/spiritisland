namespace SpiritIsland.JaggedEarth;

public class RenewingBoon{ 
		
	[MinorCard("Renewing Boon",0,Element.Sun,Element.Earth,Element.Plant),Slow,AnotherSpirit]
	[Instructions( "Choose a land where you and target Spirit both have Presence. In that land: Remove 1 Blight, and target Spirit may add 1 of their DestroyedPresence." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// Choose a land where you and target Spirit both have presence.
		var spaceOptions = ctx.Self.Presence.Lands.Intersect( ctx.Other.Presence.Lands )
			.ToArray();
		var space = await ctx.Self.Select(new A.SpaceDecision("",spaceOptions,Present.Always));
		if( space is null) return;

		// In that land: Remove 1 blight
		await ctx.Target(space).RemoveBlight();

		// and target Spirit may add 1 of their Destroyed presence.
		var otherCtx = ctx.Other.Target( space );
		if( otherCtx.Self.Presence.CanBePlacedOn(otherCtx.Space) )   // filter by the OTHER spirits placeable options
			await otherCtx.Presence.PlaceDestroyedHere(); // ! "May" Add - let them choose
	}

}