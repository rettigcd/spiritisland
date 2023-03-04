namespace SpiritIsland.Tests; 
// Test spirit that has lots of energy and you can select the card they start with

class TestSpirit : Spirit {

	public TestSpirit(PowerCard powerCard):base(
		new SpiritPresence(
			new TestPresenceTrack(Track.Energy5,Track.Energy9),
			new TestPresenceTrack(Track.Card1,Track.Card2,Track.Card3)
		)
		,powerCard
	) {
		GrowthTrack = new(
			new GrowthOption( new ReclaimAll() ) 
		);
	}

	public override string Text => "Test Spirit";

	public override SpecialRule[] SpecialRules => throw new NotImplementedException();

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Has sacred site on space 5
		Space space = board[5];
		this.Given_HasPresenceOn(space,2); 
	}

	static public (VirtualTestUser, SelfCtx) StartGame( 
		PowerCard powerCard, 
		Action<GameState> modGameState = null 
	) {
		var spirit = new TestSpirit( powerCard );
		var gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		modGameState?.Invoke( gs );

		gs.Initialize(); 

		_ = new SinglePlayer.SinglePlayerGame(gs).Start();

		var user = new VirtualTestUser( spirit );
		var starterCtx = spirit.BindSelf();

		// Disable destroying presence
		starterCtx.GameState.DisableBlightEffect();

		return (user,starterCtx);
	}

}

public class TestPresenceTrack : PresenceTrack {
	public TestPresenceTrack(params Track[] t ) : base( t ) { }
	public void OverrideTrack(int index, Track t) { slots[index]=t;}

}


public class VirtualTestUser : VirtualUser {

	public VirtualTestUser( Spirit spirit ) : base( spirit ) { }

	/// <summary> Growth for Test Spirit </summary>
	public void Grows() {
		Growth_SelectAction( "ReclaimAll" );
	}

	public void GrowAndBuyNoCards() {
		Grows();
		IsDoneBuyingCards();
	}

}

