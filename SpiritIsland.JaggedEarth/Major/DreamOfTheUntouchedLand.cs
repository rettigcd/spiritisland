namespace SpiritIsland.JaggedEarth;

public class DreamOfTheUntouchedLand {

	const string Name = "Dream of the Untouched Land";

	[MajorCard(Name,6,Element.Moon,Element.Water,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(1)]
	[Instructions( "Remove up to 3 Blight and up to 3 Health worth of Invaders.  -If you have- 3 Moon, 2 Water, 3 Earth, 2 Plant: (Max. 1x/game) Add a random new Island Board next to target board. Ignore its Setup icons; add 2 Beasts, 2 Wilds, 2 Badlands and up to 2 Presence (from any Spirits) anywhere on it. From now on, Build cards and \"Each board / Each land\" Adversary Actions skip 1 Board." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// remove up to 3 blight
		await ctx.RemoveBlight( 3 );

		// and up to 3 health worth of invaders
		await Cmd.RemoveUpToNHealthOfInvaders( 3 ).ActAsync( ctx );

		var gs = GameState.Current;

		// if you have 3 moon, 2 water  3 earth 2 plant
		if(await ctx.YouHave( "3 moon,2 water,3 earth,2 plant" )
			&& gs.Island.Boards.Length == gs.Spirits.Length
		) {

			// Add a random new island board next to target board ignore its setup icons.
			Board newBoard = PickNewRandomBoard(gs);

			// Reconfigure 1-board island to 2-board island - !! only works if starting with 1 board
			var existingBoard = gs.Island.Boards[0];

			gs.Island.AddBoard( newBoard.Sides[0], existingBoard.Sides[0] );

			//if( gs.Spirits.Length == 1  // SOLO GAME
			//	&& true // added to side 0 or 2 - HOW DO WE DETERMINE THIS?
			//) {
			//	// rotate everything 120° Counter-Clockwise to bring Ocean to bottom.
			//	var rotation = RowVector.RotateDegrees(120);
			//	gs.Island.Boards[0].Layout.ReMap(rotation);
			//	gs.Island.Boards[1].Layout.ReMap(rotation);

			//	// TODO !!! - Instead of automatically doing 120, test 0°, 30°, 60°, 90°, 120° for minimal height

			//}

			// Notify board changed.
			ActionScope.Current.Log(new Log.LayoutChanged($"{Name} added Board {newBoard.Name}"));

			// add 2 beast, 2 wilds, 2 badlands
			foreach( var token in new TokenClassToken[] { Token.Beast, Token.Wilds, Token.Badlands})
				for(int i = 0; i < 2; ++i)
					(await ctx.Self.SelectSpaceAsync($"Add {token.Label} to:", newBoard.Spaces.Where( x => !x.IsOcean ).ScopeTokens(), Present.Always ))
						.Adjust(token,1);

			// and up to 2 presence (from any Spirits) anywhere on it.
			// !!! Make sure spirits don't violate their place-presence rules?
			for(int i = 0; i < 2; ++i) {
				var spirit = await ctx.Self.SelectAsync(new A.Spirit("Spirit to add presence.", gs.Spirits,Present.AutoSelectSingle));
				await Cmd.PlacePresenceOn( newBoard.Spaces.Where( x => !x.IsOcean ).ScopeTokens().ToArray() )
					.ActAsync(spirit);
			}

			// from now on Build Cards and "Each board / Each land" Adversary Actions skip 1 board.
			gs.AddIslandMod( new InvadersSkip1Board() );

		}



	}

	static Board PickNewRandomBoard(GameState gs) {
		var rand = new Random( gs.ShuffleNumber +56127);

		// pick board
		var boardsToChooseFrom = BoardFactory.AvailableBoards.Except( gs.Island.Boards.Select( b => b.Name ) ).ToList();
		string boardName = boardsToChooseFrom[ rand.Next( boardsToChooseFrom.Count) ];

		// pick orentation
		var orientationOptions = gs.Island.AvailableConnections();
		BoardOrientation orientation = orientationOptions[rand.Next( orientationOptions.Length)];
//		BoardOrientation orientation = BoardOrientation.ToMatchSide(0, BoardOrientation.Home.SideCoord(0));

		var board = BoardFactory.Build(boardName, orientation);

		return board;
	}

}

class InvadersSkip1Board : BaseModEntity, ISkipRavages, ISkipBuilds, ISkipExploreTo, ICleanupSpaceWhenTimePasses {
	public UsageCost Cost => UsageCost.Free;
	public string Text => "Invaders Skip 1 Board";

	public async Task<bool> Skip( Space space ) {
		return space.SpaceSpec.Boards.Contains( await BoardToSkip() );
	}

	async Task<Board> BoardToSkip() {
		if(_toSkip != null) return _toSkip;

		// Select board to skip for this round.
		var gameState = GameState.Current;
		Board[] boards = gameState.Island.Boards;
		string boardToSkip = await gameState.Spirits[0].SelectText( "Select Board to skip all Invader actions", boards.Select( b => b.Name ).ToArray(), Present.Always );
		_toSkip = boards.First( b => b.Name == boardToSkip );

		return _toSkip;
	}

	public void CleanupSpace( Space space ) {
		_toSkip = null; // reset for next round.
	}

	Board? _toSkip = null;
}