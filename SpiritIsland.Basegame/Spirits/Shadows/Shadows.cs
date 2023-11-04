namespace SpiritIsland.Basegame;

public class Shadows : Spirit {

	public const string Name = "Shadows Flicker Like Flame";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("Shadows of the Dahan", "Whenever you use a power, you may pay 1 energy to target land with Dahan regardless of range.") };

	public Shadows():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.Energy3, Track.Energy4, Track.Energy5, Track.Energy6 ), 
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card3, Track.Card4, Track.Card5 )
		),
		PowerCard.For<MantleOfDread>(),
		PowerCard.For<FavorsCalledDue>(),
		PowerCard.For<CropsWitherAndFade>(),
		PowerCard.For<ConcealingShadows>()
	) {
		GrowthTrack = new(
			new GrowthOption( new ReclaimAll(), new DrawPowerCard(1) ),
			new GrowthOption( new DrawPowerCard(1), new PlacePresence(1) ),
			new GrowthOption( new PlacePresence(3), new GainEnergy(3) )
		);
		this.InnatePowers = new InnatePower[]{
			InnatePower.For<DarknessSwallowsTheUnwary>()
		};
	}

	/// <summary>
	/// Overriden so we can pay 1 energy for targetting out-of-range dahan space
	/// </summary>
	public override async Task<Space> TargetsSpace( 
		SelfCtx ctx,  // has the actual ActionScope for this Action
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria, 
		params TargetCriteria[] targetCriteria 
	) {
		var space = await base.TargetsSpace( ctx, prompt, preselect, sourceCriteria, targetCriteria );

		if( 0<Energy && !base.GetPowerTargetOptions( GameState.Current, sourceCriteria, targetCriteria ).Any( s => s.Space == space ) ) 
			--Energy;
	
		return space;
	}


	protected override IEnumerable<SpaceState> GetPowerTargetOptions(
		GameState gameState,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		var normalSpaces = base.GetPowerTargetOptions( gameState, sourceCriteria, targetCriteria );
		return Energy <= 0 
			? normalSpaces
			: normalSpaces.Union( gameState.Spaces.Where( s => s.Dahan.Any ) );
	}


	protected override void InitializeInternal( Board board, GameState gs ) {

		var higestJungle = board.Spaces.Last( s=>s.IsJungle );

		higestJungle.Tokens.Adjust(Presence.Token,2 );
		board[5].Tokens.Adjust(Presence.Token, 1);
	}

}