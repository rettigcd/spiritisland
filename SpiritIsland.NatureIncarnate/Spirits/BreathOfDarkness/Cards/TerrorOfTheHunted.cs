namespace SpiritIsland.NatureIncarnate;

public class TerrorOfTheHunted {
	const string Name = "Terror of the Hunted";

	[SpiritCard( Name, 1, Element.Moon, Element.Fire ), Slow, FromPresence(0,Filter.Invaders)] 
	[Instructions( "If Beast are present, 1 Fear and Add 1 Strife. Add 1 Strife per Terror Level. If target land is endless-dark, Add 1 Strife. (Strife only escapes with the Invader it's attached to.)" ), Artist( Artists.DavidMarkiwsky )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {

		// Add 1 Strife per Terror Level.
		int strifeCount = GameState.Current.Fear.TerrorLevel;

		// If Beast are present, 1 Fear and Add 1 Strife.
		if(ctx.Beasts.Any) {
			ctx.AddFear(1);
			++strifeCount;
		}

		// If target land is (), Add 1 Strife. (Strife only escapes with the Invader it's attached to.)
		if(ctx.Space == EndlessDark.Space)
			++strifeCount;

		await ctx.AddStrife(strifeCount);

	}

}
