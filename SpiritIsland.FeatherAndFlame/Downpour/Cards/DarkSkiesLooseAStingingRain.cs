namespace SpiritIsland.FeatherAndFlame;


public class DarkSkiesLooseAStingingRain {

	const string Name = "Dark Skies Loose a Stinging Rain";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water )]
	[Fast]
	[FromPresenceIn(1, Terrain.Wetland)]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		// Isolate target land.
		ctx.Isolate();

		// Push up to 1 explorer and up to 2 dahan
		return ctx.Pusher
			.AddGroup(1,Invader.Explorer)
			.AddGroup(2,TokenType.Dahan)
			.MoveUpToN();
	}


}

