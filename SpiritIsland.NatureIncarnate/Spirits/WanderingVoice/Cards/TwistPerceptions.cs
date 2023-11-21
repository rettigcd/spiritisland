namespace SpiritIsland.NatureIncarnate;

public class TwistPerceptions {

	public const string Name = "Twist Perceptions";

	[SpiritCard(Name,1,Element.Moon,Element.Air,Element.Animal),Slow]
	[FromPresence(1,Target.Invaders)]
	[Instructions( "Add 1 Strife. You may Push the Invader you added Strife to." ), Artist( Artists.NolanNasser )]
	static public async Task ActionAsync(TargetSpaceCtx ctx) {
		// Add 1 Strife.
		SpaceToken? strifed = null;
		await new TokenStrifer(ctx.Self,new SourceSelector(ctx.Tokens).AddGroup(1,Human.Invader))
			.Track((_,after) => strifed = after)
			.DoN();
		if(strifed == null) return;

		await TokenMover.Push(ctx.Self, strifed.Space)
			.ConfigDestinationAsOptional()
			.MoveSomewhereAsync(strifed);


	}

}