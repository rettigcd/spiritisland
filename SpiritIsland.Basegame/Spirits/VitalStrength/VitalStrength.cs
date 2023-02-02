namespace SpiritIsland.Basegame;

public class VitalStrength : Spirit {

	public const string Name = "Vital Strength of the Earth";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { EarthsVitality.Rule } ;

	public VitalStrength():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy6, Track.Energy7, Track.Energy8 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 )
		),
		PowerCard.For<GuardTheHealingLand>(),
		PowerCard.For<AYearOfPerfectStillness>(),
		PowerCard.For<RitualsOfDestruction>(),
		PowerCard.For<DrawOfTheFruitfulEarth>()
	){
		GrowthTrack = new(
			new GrowthOption( new ReclaimAll(), new PlacePresence(2) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresence(0) ),
			new GrowthOption( new GainEnergy(2), new PlacePresence(1) )
		);

		this.InnatePowers = new InnatePower[]{ 
			InnatePower.For<GiftOfStrength>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		InitPresence( board, gs );
		gs.Tokens.Dynamic.ForGame.Register( new EarthsVitality(this).DefendOnSpace, Token.Defend );
	}

	class EarthsVitality {
		static public SpecialRule Rule => new SpecialRule("Earth's Vitality","Defend 3 in every land where you have sacred site.");
		readonly SpiritPresence presence;
		public EarthsVitality( Spirit spirit ) { presence = spirit.Presence; }
		public int DefendOnSpace( SpaceState space ) 
			=> presence.IsSacredSite(space) ? 3 : 0;
	}

	void InitPresence( Board board, GameState gameState ){
		var higestJungle = gameState.Tokens[ board.Spaces.OrderByDescending( s => s.Label ).First( s => s.IsJungle ) ];
		var higestMountain = gameState.Tokens[ board.Spaces.OrderByDescending( s => s.Label ).First( s => s.IsMountain ) ];
		Presence.Adjust( higestMountain, 1 );
		Presence.Adjust( higestMountain, 1 );
		Presence.Adjust( higestJungle, 1 );
	}

}