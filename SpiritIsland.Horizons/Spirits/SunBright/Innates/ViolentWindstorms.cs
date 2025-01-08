namespace SpiritIsland.Horizons;

[InnatePower(Name,"-description-")]
[Slow,FromPresence(0,Filter.Inland)]
[RepeatIf("6 plant")]
public class ViolentWindstorms {

	public const string Name = "Violent Windstorms";

	[InnateTier("1 sun,2 air","Push up to 1 Explorer.")]
	static public Task Option1(TargetSpaceCtx ctx) {
		// Push up to 1 Explorer.
		return ctx.PushUpTo(1,Human.Explorer);
	}

	[InnateTier("2 sun,3 air","1 Fear. Push up to 2 Explorer/Town.")]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		// 1 Fear.
		await ctx.AddFear(1);
		// Push up to 2 Explorer/Town.
		await ctx.SourceSelector
			.UseQuota(new Quota()
				.AddGroup(1, Human.Explorer) // from Tier-1
				.AddGroup(2, Human.Explorer_Town)
			)
			.PushUpToN(ctx.Self);
	}

	[InnateTier("2 sun,4 air", "For each Invader Pushed by this Power, 1 Damage in the land it was Pushed to.")]
	static public async Task Option3(TargetSpaceCtx ctx) {
		// For each Invader Pushed by this Power, 1 Damage in the land it was Pushed to.
		await ctx.AddFear(1);
		List<Space> destinations = [];
		await ctx.SourceSelector
			.UseQuota(new Quota()
				.AddGroup(1, Human.Explorer) // from Tier-1
				.AddGroup(2, Human.Explorer_Town)
			)
			.Track(x => {if(x.Location is Space space) destinations.Add(space); } )
			.PushUpToN(ctx.Self);
		foreach(var dest in destinations)
			await ctx.Self.Target(dest).DamageInvaders(1);

	}

	[InnateTier("3 sun,5 air", "4 Damage (in target land)",1)]
	static public Task Option4(TargetSpaceCtx ctx) {
		// 4 Damage (in target land)
		return ctx.DamageInvaders(4);
	}

}
