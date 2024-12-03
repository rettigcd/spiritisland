namespace SpiritIsland.Tests; 
// Test spirit that has lots of energy and you can select the card they start with

class TestSpirit : Spirit {

	public override string SpiritName => "Test Spirit";

	#region constructors

	/// <summary>
	/// Spirit with Energy and Card Plays, but unknown Cards
	/// </summary>
	public TestSpirit()
		: base(x => new SpiritPresence(x,
					new TestPresenceTrack(Track.Energy5, Track.Energy9),
					new TestPresenceTrack(Track.Card1, Track.Card2, Track.Card3)
				)
			, new GrowthTrack(new GrowthGroup(new ReclaimAll()))
			, PowerCard.For(typeof(InfiniteVitality))
		) {
	}

	/// <summary>
	/// Spirit with a given card in their hand.
	/// </summary>
	public TestSpirit( PowerCard powerCard ) 
		: base(x => new SpiritPresence(x,
					new TestPresenceTrack(Track.Energy5, Track.Energy9),
					new TestPresenceTrack(Track.Card1, Track.Card2, Track.Card3)
				)
			, new GrowthTrack( new GrowthGroup( new ReclaimAll() ) )
			, powerCard
		) { }

	/// <summary> For testing a growth actions </summary>
	public TestSpirit(params IActOn<Spirit>[] growthOptions)
		: base(x => new SpiritPresence(x,
				new PresenceTrack(Track.Energy0, Track.Energy0, Track.Energy0),
				new PresenceTrack(Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5)
				)
			, new GrowthTrack( new GrowthGroup(growthOptions) )
			, PowerCard.For(typeof(InfiniteVitality))
		) { }

	#endregion constructors

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Has sacred site on space 5
		SpaceSpec space = board[5];
		this.Given_IsOn(space,2); 
	}

	[Obsolete("Use SoloGameState")]
	static public (VirtualTestUser, Spirit, Task) StartGame( 
		PowerCard powerCard, 
		Action<GameState> modGameState = null 
	) {
		var spirit = new TestSpirit( powerCard );
		var gs = new SoloGameState( spirit ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		modGameState?.Invoke( gs );

		gs.Initialize(); 

		Task task = new SinglePlayer.SinglePlayerGame(gs).StartAsync();

		var user = new VirtualTestUser( spirit );

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

		return (user,spirit,task);
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

