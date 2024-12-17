namespace SpiritIsland.Tests; 

/// <summary>
/// Used when the particular Spirit is not important.
/// </summary>
class TestSpirit : Spirit {

	public override string SpiritName => "Test Spirit";

	#region constructors

	/// <summary> Spirit with Energy and Card Plays, but unknown Cards </summary>
	public TestSpirit()
		: base(x => new SpiritPresence(x,
					new TestPresenceTrack(Track.Energy5, Track.Energy9),
					new TestPresenceTrack(Track.Card1, Track.Card2, Track.Card3)
				)
			, new GrowthTrack(new GrowthGroup(new ReclaimAll()))
			, PowerCard.ForDecorated(InfiniteVitality.ActAsync)
		) {
	}

	/// <summary> Spirit with a given card in their hand. </summary>
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
			, PowerCard.ForDecorated(InfiniteVitality.ActAsync)
		) { }

	#endregion constructors

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Has sacred site on space 5
		SpaceSpec space = board[5];
		this.Given_IsOn(space,2); 
	}

	class TestPresenceTrack(params Track[] t) : PresenceTrack(t) {
		public void OverrideTrack(int index, Track t) { _slots[index] = t; }

	}

}
