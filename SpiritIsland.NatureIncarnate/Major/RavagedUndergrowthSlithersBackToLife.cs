namespace SpiritIsland.NatureIncarnate;

public class RavagedUndergrowthSlithersBackToLife {

	public const string Name = "Ravaged Undergrowth Slithers Back to Life";

	[MajorCard(Name,3,"water,plant,animal"),Slow]
	[FromSacredSite(1,Filter.Blight)]
	[Instructions( "Replace 1 Blight with 1 Wilds. 1 Fear. 3 Damage. Push that Wilds. -If you have- 3 water,2 plant: Add 1 Wilds. You may Push it. In each land with Wilds within Range-1, Push 1 Explorer and 1 Town to lands without Wilds." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// Replace 1 Blight with 1 Wilds.
		var result = await ctx.Tokens.RemoveAsync(Token.Blight,1,RemoveReason.Replaced);
		if(result.Count == 1) await ctx.Wilds.AddAsync(1);

		// 1 Fear.
		ctx.AddFear(1);

		// 3 Damage.
		await ctx.DamageInvaders(3);

		// Push that Wilds.
		await ctx.Push(1,Token.Wilds);

		// -If you have- 3 water,2 plant:
		if(await ctx.YouHave("3 water,2 plant" )) {
			// Add 1 Wilds.
			await ctx.Wilds.AddAsync(1);
			// You may Push it.
			await ctx.PushUpTo(1,Token.Wilds);
			// In each land with Wilds within Range-1, 
			foreach(var spaceState in ctx.Tokens.Range(1).Where(t=>0<t.Wilds.Count).ToArray())
				// Push 1 Explorer and 1 Town
				await spaceState.SourceSelector
					.AddGroup(1,Human.Explorer)
					.AddGroup(1,Human.Town)
					// to lands without Wilds.
					.ConfigDestination(d=>d.FilterDestination(ss=>ss.Wilds.Count==0))
					.PushN(ctx.Self);
		}
	}

}
