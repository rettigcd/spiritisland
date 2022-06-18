﻿namespace SpiritIsland.Basegame.Adversaries;

/*

Additional Loss Condition
Sprawling Plantations: Before Setup, return all but 7 Town per player to the box. Invaders win if you ever cannot place a Town.

6	(10) 14( 4 / 5 / 5 )	
!!! Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer.
*/


public class France : IAdversary {

	public int Level { get; set; }

	public void AdjustInvaderDeck( InvaderDeck deck ) {
		deck.ReplaceCards( card => new FranceInvaderCard( card, Level ) );
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

	public void Adjust( GameState gameState ) {
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
		gameState.Tokens.TokenRemoved.ForGame.Add( args => {
			if( args.Token != TokenType.Blight
				|| args.Reason.IsOneOf(	RemoveReason.MovedFrom,	RemoveReason.Replaced )
			) return;

			if(slowBlightCount == 2) {
				gameState.blightOnCard += 2;
				slowBlightCount = 0;
			} else {
				--gameState.blightOnCard;
				++slowBlightCount;
			}
		} );
	}

	static void EarlyPlantation( GameState gameState ) {
		// During Setup, on each board
		foreach(var board in gameState.Island.Boards) {
			// add 1 Town to the highest-numbered land without Town.
			board.Spaces.Cast<Space1>().Where(s=>s.StartUpCounts.Towns == 0).Last().StartUpCounts.Adjust('T',1);
			// Add 1 Town to land #1.
			(board[0] as Space1).StartUpCounts.Adjust('T',1);
		}
	}

	static void AddSlaveRebellionEvent( GameState gameState ) {
		// !!! This should really be post-blighted island effects, but oh-well...
		gameState.StartOfInvaderPhase.ForGame.Add( async gs => {
			if( gs.RoundNumber%4 != 0) return;// if we put it under 3 cards, it will be every 4th card.
			if(gs.InvaderDeck.InvaderStage < 3) {
				// On Each Board: Add Strife to 1 Town.
				await Cmd.OnEachBoard( AddStrifeToTown ).Execute(gs);
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
					new ActionOption<GameState>(
						"each invader takes 1 damage per strife it has", 
						x => StrifedRavage.StrifedInvadersTakeDamagePerStrife( new FearCtx(x) )
					)
				).Execute(gs);
			}
		} );
	}

	static ActionOption<BoardCtx> AddStrifeToTown => new ActionOption<BoardCtx>(
		"Add a strife to a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Add strife to town", options, Present.Always ) );
			if(st != null)
				await boardCtx.GameState.Tokens[st.Space].AddStrifeTo( (HealthToken)st.Token, Guid.NewGuid(), 1 );
		} );

	static ActionOption<BoardCtx> Add2StrifeToCityOrTown => new ActionOption<BoardCtx>(
		"Add 2 strife to any city/town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town, Invader.City );
			for(int i = 0; i < 2; ++i) {
				var st = await boardCtx.Decision( new Select.TokenFromManySpaces( $"Add strife ({i+1} of 2)", options, Present.Always ) );
				if(st != null)
					await boardCtx.GameState.Tokens[st.Space].AddStrifeTo( (HealthToken)st.Token, Guid.NewGuid(), 1 );
			}
		} );

	static ActionOption<BoardCtx> DestroyTown => new ActionOption<BoardCtx>(
		"Destroy a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.FindTokens( Invader.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Destory a town", options, Present.Always ) );
			if(st != null)
				await boardCtx.GameState.Tokens[st.Space].Destroy( st.Token, 1, Guid.NewGuid() );
		} );

}

public class FranceInvaderCard : InvaderCard {

	readonly bool hasFrontierExploration;
	readonly bool hasSlaveLabor;
	readonly bool hasPersistentExplorers;
	readonly bool hasTriangleTrade;

	public FranceInvaderCard( InvaderCard original, int level ) : base( original ) { 
		hasFrontierExploration = 1 <= level;
		hasSlaveLabor = 2 <= level;
		hasTriangleTrade = 4 < level;
		hasPersistentExplorers = 6<= level;
	}

	#region Build Stuff

	protected override async Task BuildIn1Space( GameState gameState, BuildEngine buildEngine, TokenCountDictionary tokens ) { 
		int initialCityCount = tokens.Sum(Invader.City);
		await base.BuildIn1Space( gameState, buildEngine, tokens );

		// Slave Labor: 
		// After Invaders Build in a land with 2 Explorer or more, replace all but 1 Explorer there with an equal number of Town.
		if( hasSlaveLabor )
			DoSlaveLabor( tokens );

		if( hasTriangleTrade )
			await DoTriangleTrade( gameState, tokens, initialCityCount );

	}

	static async Task DoTriangleTrade( GameState gs, TokenCountDictionary tokens, int initialCityCount ) {
		// Whenever Invaders Build a Coastal City
		if(tokens.Space.IsCoastal && tokens.Sum(Invader.City) > initialCityCount) {
			// add 1 Town to the adjacent land with the fewest Town.
			var buildSpace = tokens.Space.Adjacent
				.Where(s=>!s.IsOcean)
				.Select( s=> gs.Tokens[s] )
				.OrderBy( t=>t.Sum(Invader.Town))
				.First();
			await buildSpace.AddDefault(Invader.Town,1,Guid.NewGuid());
		}
	}

	static void DoSlaveLabor( TokenCountDictionary tokens ) {
		int explorerCount = tokens.Sum( Invader.Explorer );
		if(explorerCount < 2 ) return;

		// remove explorers
		int numToReplace = explorerCount - 1;
		while(numToReplace > 0) {
			var explorerToken = tokens.OfType( Invader.Explorer ).Cast<HealthToken>().OrderByDescending( x => x.StrifeCount ).FirstOrDefault();
			int count = Math.Min( tokens[explorerToken], numToReplace );
			// Replace
			tokens.Adjust( explorerToken, -count );
			tokens.AdjustDefault( Invader.Town, count );
			// next
			numToReplace -= count;
		}
	}

	#endregion

	#region Explore stuff

	public override async Task Explore( GameState gs ) {
		// Original
		TokenCountDictionary[] tokenSpacesToExplore = await PreExplore( gs );
		await DoExplore( gs, tokenSpacesToExplore );

		if(hasFrontierExploration)
			await DoFrontierExploration( gs, tokenSpacesToExplore );

		if(HasEscalation)
			await Escalation( gs );

		if(hasPersistentExplorers)
			await PersistentExplorers( gs );
	}

	static Task PersistentExplorers( GameState gs ) {
		// After resolving an Explore Card, on each board add 1 Explorer to a land without any. 
		return Cmd.OnEachBoard( AddExplorerToLandWithoutAny ).Execute( gs );
		
	}
	static ActionOption<BoardCtx> AddExplorerToLandWithoutAny => new ActionOption<BoardCtx>(
		"Add Explorer to Land without any",
		async x => {
			var options = x.Board.Spaces.Where(s=>!x.GameState.Tokens[s].HasAny(Invader.Explorer)).ToArray();
			var space = await x.Decision( new Select.Space("Add explorer", options, Present.Always));
			if( space != null)
				await x.GameState.Tokens[space].AddDefault(Invader.Explorer, 1, Guid.NewGuid());
		}
	);

	static async Task DoFrontierExploration( GameState gs, TokenCountDictionary[] tokenSpacesToExplore ) {
		// Frontier Explorers: Except during Setup: After Invaders successfully Explore into a land which had no Town / City, add 1 Explorer there.
		foreach(var exploreTokens in tokenSpacesToExplore)
			if(!exploreTokens.HasAny( Invader.Town, Invader.City ))
				await ExploreSingleSpace( exploreTokens, gs, Guid.NewGuid() ); // !!! implemented as a new Action - Should it be???
	}

	Task Escalation( GameState gs ) {
		// Demand for New Cash Crops:
		// After Exploring, on each board, pick a land of the shown terrain.If it has Town / City, add 1 Blight.Otherwise, add 1 Town

		ActionOption<SelfCtx> SelectSpaceAction(Space s ) {
			return gs.Tokens[s].HasAny( Invader.Town, Invader.City )
				? new ActionOption<SelfCtx>( "Add 1 Town to " + s.Text, null )
				: new ActionOption<SelfCtx>( "", null );
		}

		return Cmd.OnEachBoard( new ActionOption<BoardCtx>( 
			"Place town or blight matching Explore card."
			, boardCtx => Cmd.Pick1( boardCtx.Board.Spaces
				.Where( Matches )
				.Select( SelectSpaceAction )
				.ToArray()
			).Execute( boardCtx )
		) ).Execute( gs );

	}

	#endregion
}