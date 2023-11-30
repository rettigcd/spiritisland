namespace SpiritIsland.FeatherAndFlame;

public class DarkSkiesLooseAStingingRain {

	const string Name = "Dark Skies Loose a Stinging Rain";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water ),Fast,FromPresence(Target.Wetland, 1)]
	[Instructions( "Isolate target land. Push up to 1 Explorer and up to 2 Dahan." ), Artist( Artists.DamonWestenhofer )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		// Isolate target land.
		ctx.Isolate();

		// Push up to 1 explorer and up to 2 dahan
		return ctx.SourceSelector
			.AddGroup(1,Human.Explorer)
			.AddGroup(2,Human.Dahan)
			.PushUpToN( ctx.Self );
	}


}

