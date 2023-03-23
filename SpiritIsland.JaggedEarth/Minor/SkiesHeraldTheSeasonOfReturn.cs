namespace SpiritIsland.JaggedEarth;

public class SkiesHeraldTheSeasonOfReturn{ 

	const string Name = "Skies Herald the Season of Return";

	[MinorCard(Name, 1, Element.Sun,Element.Moon,Element.Plant,Element.Animal),Fast,FromPresence(1)]
	[Instructions( "A Spirit with Presence on target board may add 1 of their Destroyed Presence. Gather up to 2 Dahan. Push 1 Blight." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// A Spirit with presence on target board may add 1 of their Destroyed presence.
		var spiritOptions = ctx.GameState.Spirits
			.Where( s => 0 < s.Presence.Destroyed )
			.Where( spirit => ctx.Space.Boards.Any(spirit.Presence.Token.IsOn) )
			.ToArray();

		var other = await ctx.Decision(new Select.ASpirit(Name,spiritOptions,Present.AutoSelectSingle) );
		if(other != null)
			await ctx.TargetSpirit( other ).OtherCtx.Target(ctx.Space).Presence.PlaceDestroyedHere();

		// Gather up to 2 dahan.
		await ctx.GatherUpToNDahan(2);
		// Push 1 blight.
		await ctx.Push(1,Token.Blight);
	}

}