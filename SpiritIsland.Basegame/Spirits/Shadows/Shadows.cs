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

	protected override PowerProgression GetPowerProgression() =>
		new (
			PowerCard.For<DarkAndTangledWoods>(),
			PowerCard.For<ShadowsOfTheBurningForest>(),
			PowerCard.For<TheJungleHungers>(), // Major
			PowerCard.For<LandOfHauntsAndEmbers>(),
			PowerCard.For<TerrifyingNightmares>(),// Major
			PowerCard.For<CallOfTheDahanWays>(),
			PowerCard.For<VisionsOfFieryDoom>()
		);


	public override async Task<Space> TargetsSpace( TargettingFrom powerType, GameState gameState, string prompt, TargetSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria ) {
		// no money, do normal
		if(Energy == 0)
			return await base.TargetsSpace( powerType, gameState, prompt, sourceCriteria, targetCriteria );

		// find normal Targetable spaces
		var normalSpaces = GetTargetOptions( powerType, gameState, sourceCriteria, targetCriteria );

		// find dahan-only spaces that are not in targetable spaces
		var dahanOnlySpaces = gameState.Island.Boards
			.SelectMany(board=>board.Spaces)
			.Where( s=>gameState.DahanOn(s).Any )
			.Except(normalSpaces)
			.ToArray();
		// no dahan-only spaces, do normal
		if(dahanOnlySpaces.Length == 0)
			return await base.TargetsSpace( powerType , gameState, prompt, sourceCriteria, targetCriteria);

		// append Target-Dahan option to end of list
		List<IOption> options = normalSpaces.Cast<IOption>().ToList();
		options.Add(new TextOption("Pay 1 energy to target land with dahan"));

		// let them select normal, or choose to pay
		IOption option = await this.Select("Select land to target.",options.ToArray(), Present.Always);

		// if they select regular space, use it
		if(option is Space space)
			return space;

		// pay 1 energy
		--Energy;

		// pick from dahan-only spaces
		return await this.Action.Decision( new Select.Space( "Target land with dahan", dahanOnlySpaces, Present.Always));
	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		var higestJungle = board.Spaces.Last( s=>s.IsJungle );

		this.Presence.PlaceOn( higestJungle, gs );
		this.Presence.PlaceOn( higestJungle, gs );
		this.Presence.PlaceOn( board[5], gs );
	}

}