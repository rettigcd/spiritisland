namespace SpiritIsland.JaggedEarth;

public class DreamOfTheUntouchedLand {

	const string Name = "Dream of the Untouched Land";

	[MajorCard(Name,6,Element.Moon,Element.Water,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// remove up to 3 blight
		await ctx.RemoveBlight( 3 );

		// and up to 3 health worth of invaders
		await Cmd.RemoveUpToNHealthOfInvaders( 3 ).Execute( ctx );

		// if you have 3 moon, 2 water  3 earth 2 plant
		if(await ctx.YouHave( "3 moon,2 water,3 earth,2 plant" )) {

			// Max. (1x/game) !!! Check only works for solo games
			if(1 < ctx.GameState.Island.Boards.Length)
				throw new NotImplementedException( "Adding boards is only implemented for islands with 1 board." );

			// Add a random new island board next to target board ignore its setup icons.
			Board newBoard = PickNewRandomBoard( ctx );

			// Reconfigure 1-board island to 2-board island - !! only works if starting with 1 board
			var existingBoard = ctx.GameState.Island.Boards[0];

			ctx.GameState.Island.AddBoard( newBoard.Sides[0], existingBoard.Sides[0] );

			// add 2 beast, 2 wilds, 2 badlands
			foreach(var token in new ISpaceEntity[] { Token.Beast, Token.Wilds, Token.Badlands})
				for(int i = 0; i < 2; ++i)
					(await ctx.SelectSpace($"Add {token} to:", newBoard.Spaces.Where( x => !x.IsOcean ) )).Tokens.Adjust(token,1);
			// and up to 2 presence (from any Spirits) anywhere on it.
			// ??? Can spirits violate their place-presence rules?
			for(int i = 0; i < 2; ++i) {
				var spirit = await ctx.Self.Gateway.Decision(new Select.ASpirit("Spirit to add presence.",ctx.GameState.Spirits));
				await spirit.PlacePresenceOn1( newBoard.Spaces.Where(x=>!x.IsOcean ).Tokens().ToArray());
			}
					
			// !!! from now on Build Cards and "Each board / Each land" Adversary Actions skip 1 board.

			// Notify board changed.
			ctx.GameState.Log( new Log.LayoutChanged($"{Name} added Board {newBoard.Name}") );

		}



	}

	static Board PickNewRandomBoard( TargetSpaceCtx ctx ) {
		var rand = new Random( ctx.GameState.ShuffleNumber +56127);
		// pick board
		var boardsToChooseFrom = Board.AvailableBoards.Except( ctx.GameState.Island.Boards.Select( b => b.Name ) ).ToList();
		string boardName = boardsToChooseFrom[ rand.Next( boardsToChooseFrom.Count) ];

		// pick orentation
		var orientationOptions = ctx.GameState.Island.AvailableConnections();
		BoardOrientation orientation = orientationOptions[rand.Next( orientationOptions.Length)];

		return Board.BuildBoard( boardName, orientation );
	}

}