namespace SpiritIsland.JaggedEarth;

public class RenewingBoon{ 
		
	[MinorCard("Renewing Boon",0,Element.Sun,Element.Earth,Element.Plant),Slow,AnotherSpirit]
	[Instructions( "Choose a land where you and target Spirit both have Presence. In that land: Remove 1 Blight, and target Spirit may add 1 of their Destroyed Presence." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// Choose a land where you and target Spirit both have presence.
		var spaceOptions = ctx.Self.Presence.Spaces.Tokens().Intersect( ctx.OtherCtx.Self.Presence.Spaces.Tokens() )
			.ToArray();
		var space = await ctx.Decision(new A.Space("",spaceOptions,Present.Always));
		if( space == null) return;

		// In that land: Remove 1 blight
		await ctx.Target(space).RemoveBlight();

		// and target Spirit may add 1 of their Destroyed presence.
		var otherCtx = ctx.OtherCtx.Target( space );
		if( otherCtx.Self.Presence.CanBePlacedOn(otherCtx.Tokens) )   // filter by the OTHER spirits placeable options
			await otherCtx.Presence.PlaceDestroyedHere(); // ! "May" Add - let them choose
	}

}