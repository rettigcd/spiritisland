namespace SpiritIsland.NatureIncarnate;

public class TwistPerceptions {

	public const string Name = "Twist Perceptions";

	[SpiritCard(Name,1,Element.Moon,Element.Air,Element.Animal),Slow]
	[FromPresence(1,Filter.Invaders)]
	[Instructions( "Add 1 Strife. You may Push the Invader you added Strife to." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		IAsyncEnumerable<SpaceToken> invadersToStrife = ctx.SourceSelector
			.UseQuota(new Quota().AddGroup(1,Human.Invader))
			.PromptForStrifingAll(ctx.Self);

		await foreach(var invader in invadersToStrife) {
			// Add 1 Strife
			SpaceToken strifed = await invader.Add1StrifeToAsync();
			// You may Push the Invader you added Strife to.
			await strifed.PushAsync(ctx.Self);
		}

	}

}