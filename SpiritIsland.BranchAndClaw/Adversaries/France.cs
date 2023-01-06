namespace SpiritIsland.Basegame.Adversaries;

/*

Additional Loss Condition
Sprawling Plantations: Before Setup, return all but 7 Town per player to the box. Invaders win if you ever cannot place a Town.

6	(10) 14( 4 / 5 / 5 )	
!!! Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer.
*/

public class France : IAdversary {

	public int Level { get; set; }

	public void PostInitialization( GameState gs ) {
		gs.InvaderDeck.Explore.Engine = new FranceExplorer(Level);
	}

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 3, 3 },
		2 => new int[] { 3, 4, 3 },
		3 => new int[] { 4, 4, 3 },
		4 => new int[] { 4, 4, 4 },
		5 => new int[] { 4, 5, 4 },
		6 => new int[] { 4, 5, 5 },
		_ => null,
	};

	public int[] InvaderCardOrder => null;

	public void PreInitialization( GameState gameState ) {
		gameState.InvaderDeck.Build.Engine = new FranceBuilder( Level );

		if( 2 <= Level)
			AddSlaveRebellionEvent( gameState );
		if( 3 <= Level)
			EarlyPlantation( gameState );
		if( 5 < Level )
			SlowHealingEcosystem( gameState );
	}

	static void SlowHealingEcosystem( GameState gameState ) {
		// When you remove Blight from the board, put it here instead of onto the Blight Card.
		// As soon as you have 3 Blight per player here, move it all back to the Blight Card.
		int slowBlightCount = 0; // !!! this doesn't save / load with Rewinds

		void DoFranceStuff( ITokenRemovedArgs args ) {
			if(args.Token != TokenType.Blight
				|| args.Reason.IsOneOf( RemoveReason.MovedFrom, RemoveReason.Replaced )
			) return;

			if(slowBlightCount == 2) {
				gameState.blightOnCard += 2;
				slowBlightCount = 0;
			} else {
				--gameState.blightOnCard;
				++slowBlightCount;
			}
		}

		gameState.AddToAllActiveSpaces( new TokenRemovedHandler("France",DoFranceStuff,true) );
	}

	static void EarlyPlantation( GameState gameState ) {
		// During Setup, on each board
		foreach(var board in gameState.Island.Boards) {
			// add 1 Town to the highest-numbered land without Town.
			var highLandWithoutTown = board.Spaces.Cast<Space1>().Where( s => s.StartUpCounts.Towns == 0 ).Last();
			gameState.Tokens[highLandWithoutTown].AdjustDefault( Invader.Town, 1 );
			// Add 1 Town to land #1.
			gameState.Tokens[board[1]].AdjustDefault( Invader.Town, 1 );
		}
	}

	static void AddSlaveRebellionEvent( GameState gameState ) {
		var gameCtx = new GameCtx( gameState, ActionCategory.Adversary );

		// !!! This should really be post-blighted island effects, but oh-well...
		gameState.StartOfInvaderPhase.ForGame.Add( async gs => {
			if( gs.RoundNumber%4 != 0) return;// if we put it under 3 cards, it will be every 4th card.
			if(gs.InvaderDeck.InvaderStage < 3) {
				// On Each Board: Add Strife to 1 Town.
				await Cmd.OnEachBoard( AddStrifeToTown ).Execute( gameCtx );
			} else {
				// On Each Board:
				await Cmd.Multiple(
					Cmd.OnEachBoard(
						Cmd.Multiple<BoardCtx>(
							"Destory 1 town, add strife to any t Town/City, then invader takes 1 Damage per Strife it has",
							DestroyTown, // Destroy 1 Town.
							Add2StrifeToCityOrTown // Add Strife to any 2 Town/City.
						)
					),
					// Then each invader takes 1 damage per strife it has.
					new DecisionOption<GameCtx>(
						"each invader takes 1 damage per strife it has", 
						StrifedRavage.StrifedInvadersTakeDamagePerStrife
					)
				).Execute( gameCtx );
			}
		} );
	}

	static DecisionOption<BoardCtx> AddStrifeToTown => new DecisionOption<BoardCtx>(
		"Add a strife to a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Add strife to town", options, Present.Always ) );
			if(st != null)
				await boardCtx.GameState.Tokens[st.Space].AddStrifeTo( (HealthToken)st.Token, boardCtx.ActionScope, 1 );
		} );

	static DecisionOption<BoardCtx> Add2StrifeToCityOrTown => new DecisionOption<BoardCtx>(
		"Add 2 strife to any city/town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town, Invader.City );
			for(int i = 0; i < 2; ++i) {
				var st = await boardCtx.Decision( new Select.TokenFromManySpaces( $"Add strife ({i+1} of 2)", options, Present.Always ) );
				if(st != null)
					await boardCtx.GameState.Tokens[st.Space].AddStrifeTo( (HealthToken)st.Token, boardCtx.ActionScope, 1 );
			}
		} );

	static DecisionOption<BoardCtx> DestroyTown => new DecisionOption<BoardCtx>(
		"Destroy a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Destory a town", options, Present.Always ) );
			if(st != null)
				await boardCtx.GameState.Tokens[st.Space].Destroy( st.Token, 1, boardCtx.ActionScope );
		} );

	public ScenarioLevel[] Adjustments => Array.Empty<ScenarioLevel>(); // !!!
}
