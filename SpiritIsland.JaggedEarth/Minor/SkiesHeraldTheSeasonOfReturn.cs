namespace SpiritIsland.JaggedEarth;

public class SkiesHeraldTheSeasonOfReturn{ 

	const string Name = "Skies Herald the Season of Return";

	[MinorCard(Name, 1, Element.Sun,Element.Moon,Element.Plant,Element.Animal),Fast,FromPresence(1)]
	[Instructions( "A Spirit with Presence on target board may add 1 of their DestroyedPresence. Gather up to 2 Dahan. Push 1 Blight." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// A Spirit with presence on target board may add 1 of their Destroyed presence.
		var spiritOptions = GameState.Current.Spirits
			.Where( s => 0 < s.Presence.Destroyed.Count )
			.Where( spirit => ctx.SpaceSpec.Boards.Any(spirit.Presence.IsOn) )
			.ToArray();

		var other = await ctx.Self.Select("Select Spirit to add Destroyed Presence", spiritOptions, Present.AutoSelectSingle );
		if(other is not null)
			await ctx.Self.Target( other ).Other.Target(ctx.SpaceSpec).Presence.PlaceDestroyedHere();

		// Gather up to 2 dahan.
		await ctx.GatherUpToNDahan(2);
		// Push 1 blight.
		await ctx.Push(1,Token.Blight);
	}

}