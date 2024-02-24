namespace SpiritIsland.Tests; 
// Test spirit that has lots of energy and you can select the card they start with

class TestSpirit( PowerCard powerCard ) 
	: Spirit( x => new SpiritPresence(x,
			new TestPresenceTrack(Track.Energy5,Track.Energy9),
			new TestPresenceTrack(Track.Card1,Track.Card2,Track.Card3)
		)
		, new GrowthTrack( new GrowthGroup( new ReclaimAll() ) )
		, powerCard
	)
{
	public override string Text => "Test Spirit";

	public override SpecialRule[] SpecialRules => throw new NotImplementedException();

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Has sacred site on space 5
		Space space = board[5];
		this.Given_IsOn(space,2); 
	}

	static public (VirtualTestUser, Spirit) StartGame( 
		PowerCard powerCard, 
		Action<GameState> modGameState = null 
	) {
		var spirit = new TestSpirit( powerCard );
		var gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		modGameState?.Invoke( gs );

		gs.Initialize(); 

		new SinglePlayer.SinglePlayerGame(gs).Start();

		var user = new VirtualTestUser( spirit );

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

		return (user,spirit);
	}

}

public class TestPresenceTrack( params Track[] t ) : PresenceTrack( t ) {
	public void OverrideTrack(int index, Track t) { _slots[index]=t;}

}


public class VirtualTestUser( Spirit spirit ) : VirtualUser( spirit ) {

	/// <summary> Growth for Test Spirit </summary>
	public void Grows() {
		Growth_SelectAction( "Reclaim All" );
	}

	public void GrowAndBuyNoCards() {
		Grows();
		IsDoneBuyingCards();
	}

}

