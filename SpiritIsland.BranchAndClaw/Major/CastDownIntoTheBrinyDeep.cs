namespace SpiritIsland.BranchAndClaw;

public class CastDownIntoTheBrinyDeep {
	const string Name = "Cast Down Into the Briny Deep";
	// https://querki.net/raw/darker/spirit-island-faq/Cast+Down+into+the+Briny+Deep

	[MajorCard( Name, 9, Element.Sun, Element.Moon, Element.Water, Element.Earth ),Slow,FromSacredSite( 1, Filter.Coastal )]
	[Instructions( "6 Fear. Destroy all Invaders. -If you have- 2 Sun, 2 Moon, 4 Water, 4 Earth: Destroy the board containing target land and everything on that board. All destroyed Blight is removed from the game instead of being returned to the Blight Card." ), Artist( Artists.JasonBehnke )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 6 fear
		await ctx.AddFear(6);
		// destroy all invaders
		await ctx.Invaders.DestroyAll(Human.Invader);

		// if you have (2 sun, 2 moon, 4 water, 4 earth):
		if(await ctx.YouHave("2 sun,2 moon,4 water,4 earth" )) {
			// Pick board
			// (from querki, if space has multiple boards, user selects.)
			var boards = ctx.SpaceSpec.Boards;
			var options = boards.Select(b=>b.Name).Order().ToArray();
			string name = await ctx.Self.SelectText("Pick Board To Destroy",options,Present.AutoSelectSingle);
			var board = boards.Single(b=>b.Name == name);

			// Destroy it
			await DestroyBoard( board );
		}
	}

	static async Task DestroyBoard( Board board ) {
		// destroy the board containing target land and everything on that board.
		// All destroyed blight is removed from the game instead of being returned to the blight card.

		var existingSpaces = board.Spaces_Existing.ScopeTokens().ToArray();
		foreach(Space space in existingSpaces )
			await space.DestroySpace();

		// Scan for tokens that are in an illegal state
		foreach(Space space in existingSpaces)
			CleanUpInvalidSpace( space );

		ActionScope.Current.Log( new Log.LayoutChanged( $"{Name} destroyed Board {board.Name}" ) );
	}

	static void CleanUpInvalidSpace( Space space ) {
		if(TerrainMapper.Current.IsInPlay( space )) return;

		HumanToken[] cleanup = space.AllHumanTokens().ToArray();
		if(cleanup.Length == 0) return;

		Space target = FindClosestInPlaySpace( space );
		foreach(HumanToken token in cleanup)
			TransferToken( space, target, token );
	}

	static void TransferToken( Space space, Space target, HumanToken token ) {
		// Place on destination
		target?.Init( token, space[token] );
		// remove from origin
		space.Init( token, 0 );
	}

	static Space FindClosestInPlaySpace( Space space ) {
		int range = 0;
		Space? target;
		var mapper = TerrainMapper.Current;
		do {
			target = space.Range( ++range ).FirstOrDefault( mapper.IsInPlay );
		}while(target == null && 20 < range);
		return target ?? GameState.Current.Spaces.First(x=>x!=space); 
	}
}