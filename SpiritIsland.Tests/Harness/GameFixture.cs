using SpiritIsland.Log;

namespace SpiritIsland.Tests;
class GameFixture {

	public Spirit spirit;
	public Board board;
	public GameState gameState;
	public VirtualUser user;
	public List<InvadersRavaged> ravages;
	public List<string> Log;
	public string LogAsString => string.Join("\r\n",Log);


	public GameFixture WithSpirit(Spirit spirit ) {
		this.spirit = spirit;
		return this;
	}

	public GameFixture Start() {
		spirit ??= new TestSpirit(PowerCard.For<WashAway>());
		board ??= Board.BuildBoardA();

		gameState = new GameState(spirit,board);
		gameState.Initialize();

		// Logging
		ravages = new List<InvadersRavaged>();
		Log = new List<string>();
		gameState.NewLogEntry += (e) => { 
			Log.Add(e.Msg());
			if(e is RavageEntry re)
				ravages.Add( re.Ravaged );
		};


		user = new VirtualUser(spirit);
		_ = new SinglePlayer.SinglePlayerGame(gameState); // Start the game 1st, (Initialize will wipe custome invader counts)
		return this;
	}

	public TargetSpaceCtx TargetSpace( string spaceLabel ) { 
		var action = gameState.StartAction( ActionCategory.Default ); // !!! nothing is disposing this
		return spirit.BindSelf()
			.Target( gameState.Spaces_Unfiltered.Single( x => x.Space.Label == spaceLabel ).Space );
	}

	public void InitRavageCard( Space space ) => gameState.InvaderDeck.Ravage.Cards.Add( space.BuildInvaderCard() );
	public void InitRavageCard( Terrain terrain ) => gameState.InvaderDeck.Ravage.Cards.Add( InvaderCard.Stage1( terrain ) );

}

