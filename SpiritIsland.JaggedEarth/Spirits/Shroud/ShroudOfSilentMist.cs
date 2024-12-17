namespace SpiritIsland.JaggedEarth;

// Slow And Silent Death

public class ShroudOfSilentMist : Spirit {

	public const string Name = "Shroud of Silent Mist";

	public override string SpiritName => Name;

	public static Track MovePresence => new Track( "Moveonepresence.png" ){ // Same as Downpour
		Action = new MovePresence(1),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};

	public ShroudOfSilentMist():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(1, Track.Energy0,Track.Energy1,Track.WaterEnergy,Track.Energy2,Track.AirEnergy),
			new PresenceTrack(1, Track.Card1,Track.Card2,MovePresence,Track.MoonEnergy,Track.Card3,Track.Card4,Track.CardReclaim1,Track.Card5)
		)
		,new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard() ),
			new GrowthGroup( new PlacePresence( 0 ), new PlacePresence( 0 ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 3, Filter.Mountain, Filter.Wetland ) )
		)
		,PowerCard.ForDecorated(FlowingAndSilentFormsDartBy.ActAsync)
		,PowerCard.ForDecorated(UnnervingPall.ActAsync)
		,PowerCard.ForDecorated(DissolvingVapors.ActAsync)
		,PowerCard.ForDecorated(TheFogClosesIn.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(SuffocatingShroud)), 
			InnatePower.For(typeof(LostInTheSwirlingHaze))
		];
		SpecialRules = [GatherPowerFromTheCoolAndDark.Rule, MistsShiftAndFlow.Rule, SlowAndSilentDeath.Rule];

		Targetter = new MistsShiftAndFlow(this);
		Draw = new GatherPowerFromTheCoolAndDark(this);
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {

		gameState.AddTimePassesAction(new SlowAndSilentDeath(this));

		// Place presence in:
		// (a) Highest # mountains,
		board.Spaces.Where(s => s.IsMountain).Last().ScopeSpace.Setup(Presence.Token, 1);
		// (b) highest # wetlands
		board.Spaces.Where(s => s.IsWetland).Last().ScopeSpace.Setup(Presence.Token, 1);

	}

}
