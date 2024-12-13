namespace SpiritIsland.NatureIncarnate;

public class BlazingIntimidation {

	public const string Name = "Blazing Intimidation";

	[SpiritCard(Name,2,Element.Fire,Element.Plant,Element.Animal),Fast]
	[FromPresence(1)]
	[Instructions( "1 Fear. Push 2 Explorer/Town to a land without Incarna." ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActionAsync(TargetSpaceCtx ctx) {
		// 1 Fear.
		await ctx.AddFear(1);

		// Push 2 Explorer/Town to a land without (your) Incarna.
		await ctx.SourceSelector
			.AddGroup(2,Human.Explorer_Town)
			.ConfigDestination( d=>d.FilterDestination( ss => ss != ctx.Self.Incarna.Space ) )
			.PushN(ctx.Self);
	}

}