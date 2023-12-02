namespace SpiritIsland.NatureIncarnate;

public class EmergeFromTheDreadNightWind {
	const string Name = "Emerge from the Dread Night Wind";

	[SpiritCard( Name, 1, Element.Moon, Element.Air ), Slow, AnyLand]
	[Instructions( "Add/Move Incarna to target land. 1 Fear. If exactly 1 Invader is present, Abduct it. Otherwise, Push up to 2 Explorer/Town to different lands. Push up to 2 Dahan" ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Add / Move Incarna to target land.
		await ctx.Self.Incarna.MoveTo(ctx.Space, true);

		// 1 Fear.
		ctx.AddFear(1);

		// If exactly 1 Invader is present,
		if(ctx.Tokens.SumAny( Human.Invader ) == 1)
			// Abduct it.
			await ctx.Tokens.SpaceTokensOfAnyTag( Human.Invader ).Single().MoveTo(EndlessDark.Space);
		else // otherwise
			// Push up to 2 Explorer / Town to different lands.
			await ctx.SourceSelector
				.AddGroup( 2, Human.Explorer_Town )
				.ConfigDestination( Distribute.ToAsManyLandsAsPossible )
				.PushUpToN( ctx.Self );

		// Push up to 2 Dahan
		await ctx.PushUpToNDahan(2);
	}
}
