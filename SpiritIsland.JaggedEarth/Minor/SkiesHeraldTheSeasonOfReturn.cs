namespace SpiritIsland.JaggedEarth;

public class SkiesHeraldTheSeasonOfReturn{ 

	const string Name = "Skies Herald the Season of Return";

	[MinorCard(Name, 1, Element.Sun,Element.Moon,Element.Plant,Element.Animal),Fast,FromPresence(1)]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// A Spirit with presence on target board may add 1 of their Destroyed presence.
		var spiritOptions = ctx.GameState.Spirits.Where( s=>s.Presence.Spaces( ctx.GameState ).Any(s=>s.Board==ctx.Space.Board) && 0 < s.Presence.Destroyed ).ToArray();

		var other = await ctx.Decision(new Select.Spirit(Name,spiritOptions,Present.AutoSelectSingle) );
		if(other != null)
			await ctx.TargetSpirit( other ).OtherCtx.Target(ctx.Space).Presence.PlaceDestroyedHere();

		// Gather up to 2 dahan.
		await ctx.GatherUpToNDahan(2);
		// Push 1 blight.
		await ctx.Push(1,TokenType.Blight);
	}

}