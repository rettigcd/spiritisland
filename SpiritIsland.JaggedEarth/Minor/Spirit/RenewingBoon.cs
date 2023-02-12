namespace SpiritIsland.JaggedEarth;

public class RenewingBoon{ 
		
	[MinorCard("Renewing Boon",0,Element.Sun,Element.Earth,Element.Plant),Slow,AnotherSpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// Choose a land where you and target Spirit both have presence.
		var spaceOptions = ctx.Self.Presence.SpaceStates.Intersect( ctx.OtherCtx.Self.Presence.SpaceStates )
			.ToArray();
		var space = await ctx.Decision(new Select.ASpace("",spaceOptions,Present.Always));
		if( space == null) return;

		// In that land: Remove 1 blight
		await ctx.Target(space).RemoveBlight();

		// and target Spirit may add 1 of their Destroyed presence.
		var otherCtx = ctx.OtherCtx.Target( space );
		if( otherCtx.Self.Presence.CanBePlacedOn(otherCtx.Tokens) )   // filter by the OTHER spirits placeable options
			await otherCtx.Presence.PlaceDestroyedHere(); // ! "May" Add - let them choose
	}

}