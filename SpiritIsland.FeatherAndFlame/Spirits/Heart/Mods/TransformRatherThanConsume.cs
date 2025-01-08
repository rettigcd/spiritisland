namespace SpiritIsland.FeatherAndFlame;

public class TransformRatherThanConsume(Spirit spirit) : DestructiveNature(spirit) {
	public const string Name = "Transform Rather Than Consume";
	const string Description = "When your Actions would Destroy Cities(including by Damage), instead: 2 Fear. Replace that City with 1 Town and 1 Explorer.Push both Invaders. When your Actions would Destroy Towns(including by Damage), instead: 1 Fear.Replace that Town with 2 Explorers.Push both Invaders.";
	static public new SpecialRule Rule => new SpecialRule(Name, Description);

	public override async Task ModifyRemovingAsync(RemovingTokenArgs args) {
		await base.ModifyRemovingAsync(args);

		// When your Actions would Destroy
		if( ActionScope.Current.Owner == Self && args.Reason.IsDestroy() && args.From is Space space && args.Token is HumanToken invader ) {

			// ...Cities(including by Damage), 
			if( invader.HasTag(Human.City) ) {
				// instead:
				// 2 Fear.
				await  space.AddFear(2*args.Count);
				while( 0 < args.Count ) {
					--args.Count;
					await SplitApartAndPush(space,invader);
				}
			}
			// ...Towns(including by Damage), 
			if( invader.HasTag(Human.Town) ) {
				// instead: 1 Fear.Replace that Town with 2 Explorers.Push both Invaders.
				await space.AddFear(args.Count);
				while( 0 < args.Count ) {
					// instead:
					--args.Count;
					await SplitApartAndPush(space, invader);
				}
			}
		}

	}

	async Task SplitApartAndPush(Space space, HumanToken tokenToSplit) {
		// Replace that City with 1 Town and 1 Explorer
		var downgraded = await ReplaceInvader.DowngradeSelectedInvader(space, tokenToSplit);
		var explorer = await space.AddDefaultAsync(Human.Explorer, 1, AddReason.AsReplacement);
		// Push both Invaders.
		await space.SourceSelector
			.UseQuota(new Quota()
				.AddGroup(1, downgraded!.AsHuman().HumanClass) // !! not exactly correct, need to use exact token, not its class.
				.AddGroup(1, explorer.Added.AsHuman().HumanClass)
			)
			.PushN(Self);
	}
}
