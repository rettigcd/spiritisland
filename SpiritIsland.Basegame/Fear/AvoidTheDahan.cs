namespace SpiritIsland.Basegame;

public class AvoidTheDahan : IFearOptions {

	public const string Name = "Avoid the Dahan";
	string IFearOptions.Name => Name;

	[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
	public Task Level1( GameCtx ctx ) {
		static bool DahanMin2( GameCtx _, SpaceState space ) => space.SumAny( Invader.City, Invader.Town ) < space.Dahan.Count;
		ctx.GameState.AdjustTempTokenForAll( new SkipExploreTo_Custom( Name, true, DahanMin2 ) );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
	public Task Level2( GameCtx ctx ) {

		static bool DahanOutNumberBuildings( GameCtx _, SpaceState space, TokenClass _1 )
			=> space.SumAny( Invader.City, Invader.Town ) < space.Dahan.Count;

		ctx.GameState.AdjustTempTokenForAll( new SkipBuild_Custom( Name, true, DahanOutNumberBuildings ) );

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
	public Task Level3( GameCtx ctx ) {

		ctx.GameState.AdjustTempTokenForAll( new SkipBuild_Custom( Name, true, (_,space,_1) => space.Dahan.Any ) );
		return Task.CompletedTask;
	}

}
