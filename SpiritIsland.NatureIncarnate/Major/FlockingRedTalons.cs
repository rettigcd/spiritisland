namespace SpiritIsland.NatureIncarnate;

public class FlockingRedTalons {

	public const string Name = "Flocking Red-Talons";

	[MajorCard(Name,3,"air,water,plant,animal"),Fast]
	[FromPresence(Filter.Wetland,3)]
	[Instructions( "Add 1 Beast. Move up to 2 Beast within Range-3 to target land. For each Beast present, choose a different Invader, 1 Damage to each of those. Push 1 Explorer/Town per Beast. -If you have- 2 air,2 plant,3 animal: Repeat this Power on a different land within Range-3 of target land." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		await DoIt(ctx);

		// -If you have- 2 air,2 plant,3 beast:
		if(await ctx.YouHave("2 air,2 plant,3 animal" )) {
			// Repeat this Power on a different land within Range-3 of target land.
			Space? second = await ctx.Self.Select(new A.SpaceDecision("Repeat power on:", ctx.Space.Range(3).Where(x=>x!=ctx.Space),Present.Always));
			if(second is not null)
				await DoIt(ctx.Target(second));
		}
	}


	static async Task DoIt( TargetSpaceCtx ctx ) {
		// Add 1 Beast.
		await ctx.Beasts.AddAsync(1);

		// Move up to 2 Beast within Range-3 to target land.
		await new TokenMover(ctx.Self,"Move",ctx.Space.Range(3),ctx.Space)
			.AddGroup(2,Token.Beast)
			.DoUpToN();

		// For each Beast present, choose a different Invader, 1 Damage to each of those.
		int beastCount = ctx.Space.Beasts.Count;
		await ctx.SourceSelector
			.AddAll(Human.Invader)
			.ConfigOnlySelectEachOnce()
			.DoDamageAsync(ctx.Self,beastCount,Present.Always);

		// Push 1 Explorer/Town per Beast.
		await ctx.Push(beastCount,Human.Explorer_Town);

	}
}
