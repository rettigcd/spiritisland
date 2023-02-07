namespace SpiritIsland.Basegame;

/// <summary> River Surges in Sunlight </summary>
public class RiverSurges : Spirit {

	public const string Name = "River Surges in Sunlight";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("Rivers Domain", "Your presense in wetlands count as sacred.") };

	public override string Text => Name;

	public RiverSurges():base(
		new RiverPresence(
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card4, Track.Card5 )
		),
		PowerCard.For<BoonOfVigor>(),
		PowerCard.For<FlashFloods>(),
		PowerCard.For<RiversBounty>(),
		PowerCard.For<WashAway>()
	){
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new DrawPowerCard(1),
				new GainEnergy(1)
			),
			new GrowthOption(
				new PlacePresence( 1 ),
				new PlacePresence( 1 )
			),
			new GrowthOption( 
				new DrawPowerCard( 1 ),
				new PlacePresence( 2 ) 
			)
		);

		InnatePowers = new InnatePower[]{
			InnatePower.For<MassiveFlooding>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		gs.Tokens[board.Spaces.Reverse().First(s => s.IsWetland)].Adjust(Presence.Token, 1);
	}

}

public class RiverPresence : SpiritPresence {
	public RiverPresence( PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) { }

	// !!! Combine both of these SS tests into 1 so that we don't have to test both of them
	// AND so they both use TerrainMapper

	public override bool IsSacredSite( SpaceState space ) => base.IsSacredSite( space ) 
		|| (1 <= space[Token] && space.Space.IsWetland);  // !!! will this detect 2 of Downpours presence as wetland? Not using TerrainMapper!

	public override IEnumerable<SpaceState> SacredSiteStates{
		get {
			var scope = UnitOfWork.Current;
			return GameState.Current.AllActiveSpaces
				.Where( s => scope.TerrainMapper.MatchesTerrain( s, Terrain.Wetland ) && IsOn( s ) )
				.Union( base.SacredSiteStates )
				.Distinct();
		}
	}

}