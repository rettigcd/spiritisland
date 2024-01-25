namespace SpiritIsland.Basegame.Adversaries;


public class France : AdversaryBase, IAdversary {

	public override AdversaryLossCondition LossCondition => Loss_SprawlingPlantations;

	public override AdversaryLevel[] Levels { get; } = [
		Esc_DemandForNewCashCrops,
		L1_FrontierExplorers,
		L2_SlaveLabor,
		L3_EarlyPlantation,
		L4_TriangleTrade,
		L5_SlowHealingEcosystem,
		L6_PersistentExplorers
	];

	#region Loss Condition - Sprawling Plantations

	static AdversaryLossCondition Loss_SprawlingPlantations => new AdversaryLossCondition(
		"Sprawling Plantations: Before Setup, return all but 7 Town per player to the box. Invaders win if you ever cannot place a Town.",
		SprawlingPlantations_Imp
	);

	static void SprawlingPlantations_Imp( GameState gs ) {
		// !!! Additional Loss Condition
		// !!! Sprawling Plantations: Before Setup, return all but 7 Town per player to the box. Invaders win if you ever cannot place a Town.
	}

	#endregion Loss Condition - Sprawling Plantations

	#region Escalation - Demand for New Cash Crops

	static AdversaryLevel Esc_DemandForNewCashCrops => new AdversaryLevel(0, 2 , 3,3,3, "Demand for New Cash Crops", 
		"After Exploring, on each board, pick a land of the shown terrain.  If it has Town/City, add 1 Blight. Otherwise, add 1 Town."
	) {
		InitFunc = (gs,_) => {
			gs.InvaderDeck.Explore.Engine.Escalation = DemandForNewCashCrops;
		}
	};

	public static async Task DemandForNewCashCrops( GameState gs ) {
		// After Exploring, on each board, pick a land of the shown terrain.If it has Town / City, add 1 Blight.Otherwise, add 1 Town

		static SpiritAction SelectSpaceAction( SpaceState s ) {
			return s.HasAny( Human.Town_City )
				? new SpiritAction( "Add 1 Town to " + s.Space.Text, null ) // !!! implement
				: new SpiritAction( "", null ); // !!! implement
		}

		var card = gs.InvaderDeck.Explore.Cards.First(); // !!! ??? is this correct

		await using var scope = await ActionScope.Start(ActionCategory.Adversary);

		await Cmd.ForEachBoard( new BaseCmd<BoardCtx>(
			"Place town or blight matching Explore card."
			, boardCtx => Cmd.Pick1( 
				boardCtx.Board.Spaces.Tokens()
					.Where( card.MatchesCard )
					.Select( SelectSpaceAction )
					.ToArray()
			).ActAsync( boardCtx.Self )
		) ).ActAsync( gs );

	}

	#endregion Escalation - Demand for New Cash Crops

	#region Level 1 - Frontier Explorers

	static AdversaryLevel L1_FrontierExplorers => new AdversaryLevel(1, 3, 3,3,3, "Frontier Explorers", 
		"Except during Setup: After Invaders successfullly Explore into a land which has not Town/City, add 1 Explorer there." 
	){
		InitFunc = (gs,adv)=>{
			gs.InvaderDeck.Explore.Engine = new FranceExplorer( 
				hasFrontierExploration: 1 <= adv.Level, // Level 1 stuff
				hasPersistentExplorers: 6 <= adv.Level  // Level 6 stuff
			);
		},
	};

	#endregion Level 1 - Frontier Explorers

	#region Level 2 - Slave Labor

	static AdversaryLevel L2_SlaveLabor => new AdversaryLevel(2, 5, 3,4,3, "Slave Labor", 
		"During Setup, put the 'Slave Rebellion' event under the top 3 cards of the Event Deck.  After Invaders Buid in a land with 2 Explorer or more, replace all but 1 Explorer there with an equal number of Town." ) {
		InitFunc = (gameState,adv) => { 
			gameState.AddPreInvaderPhaseAction( new SlaveRebellion() );
			gameState.InvaderDeck.Build.Engine = new FranceBuilder( 
				_hasSlaveLabor: 2 <= adv.Level, // Level 2 stuff
				_hasTriangeTrade: 4 < adv.Level // Level 4 stuff
			);

		}
	};

	class SlaveRebellion : IRunBeforeInvaderPhase {
		bool IRunBeforeInvaderPhase.RemoveAfterRun => false;
		async Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState) {

			if(gameState.RoundNumber % 4 != 0) return;// if we put it under 3 cards, it will be every 4th card.

			BaseCmd<BoardCtx> cmd = (gameState.InvaderDeck.InvaderStage < 3)
				? AddStrifeToTown
				: Cmd.Multiple<BoardCtx>(
					"Destory 1 town, add strife to any 2 Town/City, then invader takes 1 Damage per Strife it has",
					DestroyTown,
					Add2StrifeToCityOrTown,
					StrifedRavage.StrifedInvadersTakeDamagePerStrife
				);

			await using var actionScope = await ActionScope.Start( ActionCategory.Adversary );
			await cmd.ForEachBoard().ActAsync( gameState );
		}
	}

	static BaseCmd<BoardCtx> AddStrifeToTown => new BaseCmd<BoardCtx>(
		"Add a strife to a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town );
			var st = await boardCtx.Self.SelectAsync( new A.SpaceToken( "Add strife to town", options, Present.Always ) );
			if(st != null)
				await st.Add1StrifeToAsync();
		} );

	static BaseCmd<BoardCtx> Add2StrifeToCityOrTown => new BaseCmd<BoardCtx>(
		"Add 2 strife to any city/town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town_City );
			for(int i = 0; i < 2; ++i) {
				var st = await boardCtx.SelectAsync( new A.SpaceToken( $"Add strife ({i+1} of 2)", options, Present.Always ) );
				if(st != null)
					await st.Add1StrifeToAsync();
			}
		} );

	static BaseCmd<BoardCtx> DestroyTown => new BaseCmd<BoardCtx>(
		"Destroy a town"
		, async boardCtx => {
			SpaceToken[] options = boardCtx.Board.FindTokens( Human.Town );
			var st = await boardCtx.SelectAsync( new A.SpaceToken( "Destory a town", options, Present.Always ) );
			if(st != null)
				await st.Space.Tokens.Destroy( st.Token, 1 );
		} );


	#endregion Level 2 - Slave Labor

	#region Level 3 - Early Plantation

	static AdversaryLevel L3_EarlyPlantation => new AdversaryLevel(3, 7, 4,4,3, "Early Plantation", 
		"During Setup, on each board add 1 Town to the highest-numbered land without Town.  Add 1 Town to land #1." ) {
		InitFunc = (gameState,_) => {
			// During Setup, on each board
			foreach(var board in gameState.Island.Boards) {
				// add 1 Town to the highest-numbered land without Town.
				var highLandWithoutTown = board.Spaces.Cast<Space1>().Where( s => s.StartUpCounts.Towns == 0 ).Last();
				highLandWithoutTown.Tokens.Setup( Human.Town, 1 );
				// Add 1 Town to land #1.
				board[1].Tokens.Setup( Human.Town, 1 );
			}
		}
	};

	#endregion Level 3 - Early Plantation

	#region Level 4 - Triangle Trade

	static AdversaryLevel L4_TriangleTrade => new AdversaryLevel(4, 8, 4,4,4, "Triangle Trade",  "Whenever Invaders Build a Coastal City, add 1 Town to the adjacent land with the fewest Town." );

	#endregion Level 4 - Triangle Trade

	#region Level 5 - Slow-Healing Ecosystem

	static AdversaryLevel L5_SlowHealingEcosystem => new AdversaryLevel(5, 9 , 4,5,4, "Slow-Healing Ecosystem",  
			"When you remove Blight fomr the board, put it here instead of onto the Blight Card. As soon as you have 3 Blight per player here, move it all back to the Blight Card." ) {
			InitFunc = (gameState,_) => {
				// When you remove Blight from the board, put it here instead of onto the Blight Card.
				// As soon as you have 3 Blight per player here, move it all back to the Blight Card.

				async Task DoFranceStuff( ITokenRemovedArgs args ) {
					if(args.Removed != Token.Blight || args.Reason.IsOneOf( RemoveReason.MovedFrom, RemoveReason.Replaced ) ) return;

					var slowBlight = FrancePanel.Tokens.Blight;
					var blightCard = BlightCard.Space.Tokens;
					if(slowBlight.Count+1 == 3*gameState.Spirits.Length) {
						await blightCard.AddAsync(Token.Blight,1);
						slowBlight.Init(0);
					} else {
						await blightCard.RemoveAsync(Token.Blight,1);
						slowBlight.Adjust(1);
					}
				}

				gameState.AddIslandMod( new TokenRemovedHandlerAsync_Persistent(DoFranceStuff) );

			}
		};
	static readonly FakeSpace FrancePanel = new FakeSpace( "FranceAdversaryPanel" ); // stores slow blight

	#endregion Level 5 - Slow-Healing Ecosystem

	#region Level 6 - Persistent Explorers

	static AdversaryLevel L6_PersistentExplorers => new AdversaryLevel(6, 10, 4,5,5, "Persistent Explorers", 
		"After resolving an Explore Card, on each board add 1 Explorer to a land without any.  Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer." 
	) {
		InitFunc = (gameState,_) => gameState.AddIslandMod( new FranceFearPushesExplorers() )
	};


	#endregion Level 6 - Persistent Explorers

}
