namespace SpiritIsland.Basegame;

public class Shadows : Spirit {

	public const string Name = "Shadows Flicker Like Flame";
	public override string SpiritName => Name;

	public const string ShadowsOfTheDahan_Name = "Shadows of the Dahan";
	static readonly SpecialRule ShadowsOfTheDahan = new SpecialRule(
		ShadowsOfTheDahan_Name, 
		"Whenever you use a power, you may pay 1 energy to target land with Dahan regardless of range.");

	public Shadows():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.Energy3, Track.Energy4, Track.Energy5, Track.Energy6 ), 
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card3, Track.Card4, Track.Card5 )
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard() ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 1 ) ),
			new GrowthGroup( new PlacePresence( 3 ), new GainEnergy( 3 ) )
		),
		PowerCard.For(typeof(MantleOfDread)),
		PowerCard.For(typeof(FavorsCalledDue)),
		PowerCard.For(typeof(CropsWitherAndFade)),
		PowerCard.For(typeof(ConcealingShadows))
	) {
		InnatePowers = [ InnatePower.For(typeof(DarknessSwallowsTheUnwary)) ];
		SpecialRules = [ShadowsOfTheDahan];
	}

	/// <summary>
	/// Overriden so we can pay 1 energy for targetting out-of-range dahan space
	/// </summary>
	public override async Task<(Space, Space[])> TargetsSpace( 
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria, 
		params TargetCriteria[] targetCriteria 
	) {
		var (space,sources) = await base.TargetsSpace( prompt, preselect, sourceCriteria, targetCriteria );

		if( 0<Energy && !base.GetPowerTargetOptions( GameState.Current, sourceCriteria, targetCriteria ).options.Any( s => s == space ) ) 
			--Energy;
	
		return (space,sources);
	}


	protected override (Space[] sources, Space[] options) GetPowerTargetOptions(
		GameState gameState,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		var (sources,normalSpaces) = base.GetPowerTargetOptions( gameState, sourceCriteria, targetCriteria );
		return (
			sources,
			Energy <= 0 ? normalSpaces : normalSpaces.Union(ActionScope.Current.Spaces.Where( s => s.Dahan.Any ) ).ToArray()
		);
	}


	protected override void InitializeInternal( Board board, GameState gs ) {

		var higestJungle = board.Spaces.Last( s=>s.IsJungle );

		higestJungle.ScopeSpace.Setup(Presence.Token,2 );
		board[5].ScopeSpace.Setup(Presence.Token, 1);
	}

}