namespace SpiritIsland.Basegame.Adversaries;

/*

Additional Loss Condition
Sprawling Plantations: Before Setup, return all but 7 Town per player to the box. Invaders win if you ever cannot place a Town.

*/

public class France : IAdversary {

	public int Level { get; set; }

	public void PostInitialization( GameState gs ) {}

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 3, 3 },
		2 => new int[] { 3, 4, 3 },
		3 => new int[] { 4, 4, 3 },
		4 => new int[] { 4, 4, 4 },
		5 => new int[] { 4, 5, 4 },
		6 => new int[] { 4, 5, 5 },
		_ => null,
	};

	public InvaderDeckBuilder InvaderDeckBuilder => InvaderDeckBuilder.Default;

	public void PreInitialization( GameState gameState ) {
		gameState.InvaderDeck.Build.Engine = new FranceBuilder( Level );
		gameState.InvaderDeck.Explore.Engine = new FranceExplorer( Level );


		if( 2 <= Level)
			AddSlaveRebellionEvent( gameState );
		if( 3 <= Level)
			EarlyPlantation( gameState );
		if( 5 <= Level )
			SlowHealingEcosystem( gameState );
		if( 6 <= Level )
			gameState.AddIslandMod( new FearPushesExplorers() );
	}

	static readonly FakeSpace FrancePanel = new FakeSpace( "FranceAdversaryPanel" ); // stores slow blight

	static void SlowHealingEcosystem( GameState gameState ) {
		// When you remove Blight from the board, put it here instead of onto the Blight Card.
		// As soon as you have 3 Blight per player here, move it all back to the Blight Card.

		async Task DoFranceStuff( ITokenRemovedArgs args ) {
			if(args.Removed != Token.Blight || args.Reason.IsOneOf( RemoveReason.MovedFrom, RemoveReason.Replaced ) ) return;

			var slowBlight = FrancePanel.Tokens.Blight;
			var blightCard = BlightCard.Space.Tokens;
			if(slowBlight.Count+1 == 3*gameState.Spirits.Length) {
				await blightCard.Add(Token.Blight,1);
				slowBlight.Init(0);
			} else {
				await blightCard.Remove(Token.Blight,1);
				slowBlight.Adjust(1);
			}
		}

		gameState.AddIslandMod( new TokenRemovedHandlerAsync_Persistent(DoFranceStuff) );
	}

	static void EarlyPlantation( GameState gameState ) {
		// During Setup, on each board
		foreach(var board in gameState.Island.Boards) {
			// add 1 Town to the highest-numbered land without Town.
			var highLandWithoutTown = board.Spaces.Cast<Space1>().Where( s => s.StartUpCounts.Towns == 0 ).Last();
			highLandWithoutTown.Tokens.AdjustDefault( Human.Town, 1 );
			// Add 1 Town to land #1.
			board[1].Tokens.AdjustDefault( Human.Town, 1 );
		}
	}

	static void AddSlaveRebellionEvent( GameState gameState ) {
		
		gameState.StartOfInvaderPhase.Add( async gs => {
			if( gs.RoundNumber%4 != 0) return;// if we put it under 3 cards, it will be every 4th card.

			DecisionOption<BoardCtx> cmd = (gs.InvaderDeck.InvaderStage < 3)
				? AddStrifeToTown
				: Cmd.Multiple<BoardCtx>(
					"Destory 1 town, add strife to any 2 Town/City, then invader takes 1 Damage per Strife it has",
					DestroyTown,
					Add2StrifeToCityOrTown,
					StrifedRavage.StrifedInvadersTakeDamagePerStrife
				);

			await using var actionScope = new ActionScope( ActionCategory.Adversary );
			GameCtx gameCtx = new GameCtx( gameState );
			await cmd.ForEachBoard().Execute( gameCtx );

		} );
	}

	static DecisionOption<BoardCtx> AddStrifeToTown => new DecisionOption<BoardCtx>(
		"Add a strife to a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Add strife to town", options, Present.Always ) );
			if(st != null)
				await st.Space.Tokens.Add1StrifeTo( st.Token.AsHuman() );
		} );

	static DecisionOption<BoardCtx> Add2StrifeToCityOrTown => new DecisionOption<BoardCtx>(
		"Add 2 strife to any city/town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town_City );
			for(int i = 0; i < 2; ++i) {
				var st = await boardCtx.Decision( new Select.TokenFromManySpaces( $"Add strife ({i+1} of 2)", options, Present.Always ) );
				if(st != null)
					await st.Space.Tokens.Add1StrifeTo( st.Token.AsHuman() );
			}
		} );

	static DecisionOption<BoardCtx> DestroyTown => new DecisionOption<BoardCtx>(
		"Destroy a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town );
			var st = await boardCtx.Decision( new Select.TokenFromManySpaces( "Destory a town", options, Present.Always ) );
			if(st != null)
				await st.Space.Tokens.Destroy( st.Token, 1 );
		} );

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		// Level 0 - Escalation
		new ScenarioLevel(2 , 3,3,3, "Demand for New Cash Crops", "After Exploring, on each board, pick a land of the shown terrain.  If it has Town/City, add 1 Blight. Otherwise, add 1 Town." ),
		// Level 1
		new ScenarioLevel(3 , 3,3,3, "Frontier Explorers", "Except during Setup: After Invaders successfullly Explore into a land which has not Town/City, add 1 Explorer there." ),
		// Level 2
		new ScenarioLevel(5 , 3,4,3, "Slave Labor", "During Setup, put the 'Slave Rebellion' event under the top 3 cards of the Event Deck.  After Invaders Buid in a land with 2 Explorer or more, replace all but 1 Explorer there with an equal number of Town." ),
		// Level 3
		new ScenarioLevel(7 , 4,4,3, "Early Plantation", "During Setup, on each board add 1 Town to the highest-numbered land without Town.  Add 1 Town to land #1." ),
		// Level 4
		new ScenarioLevel(8 , 4,4,4, "Triangle Trade",  "Whenever Invaders Build a Coastal City, add 1 Town to the adjacent land with the fewest Town." ),
		// Level 5
		new ScenarioLevel(9 , 4,5,4, "Slow-Healing Ecosystem",  "When you remove Blight fomr the board, put it here instead of onto the Blight Card. As soon as you have 3 Blight per player here, move it all back to the Blight Card." ),
		// Level 6
		new ScenarioLevel(10, 4,5,5, "Persistent Explorers", "After resolving an Explore Card, on each board add 1 Explorer to a land without any.  Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer." ),
	};

}

class FearPushesExplorers : BaseModEntity, IModifyRemovingTokenAsync {

	// Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer.
	public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		if(args.Token.Class == Human.Explorer 
			&& args.Reason == RemoveReason.Removed
			&& ActionScope.Current.Category == ActionCategory.Fear
		) {
			Spirit spirit = args.From.Space.Boards.First().FindSpirit();

			SpaceState[] destinationOptions = args.From.Adjacent_ForInvaders.ToArray();

			var pusher = new TokenPusher( spirit, args.From )
				.FilterDestinations( destinationOptions.Contains );

			int attempts = args.Count; // generally this is 1
			while(0 < attempts--) {
				var space = await pusher.PushToken( args.Token ); // ? Can we reuse a pusher?
				// if push was successful, don't remove
				if( space != null ) --args.Count;
			}
		}
	}
}
