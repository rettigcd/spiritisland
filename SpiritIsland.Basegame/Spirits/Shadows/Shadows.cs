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
		TargetingPowerType targetingPowerType, 
		SelfCtx ctx,  // has the actual ActionId for this Action
		string prompt, 
		TargetingSourceCriteria sourceCriteria, 
		params TargetCriteria[] targetCriteria 
	) {
		// no money, do normal
		if(Energy == 0)
			return await base.TargetsSpace( targetingPowerType, ctx, prompt, sourceCriteria, targetCriteria );

		// find normal Targetable spaces
		var normalSpaces = GetPowerTargetOptions( targetingPowerType, ctx.GameState, sourceCriteria, targetCriteria );

		// find dahan-only spaces that are not in targetable spaces
		var dahanOnlySpaces = ctx.GameState.AllActiveSpaces
			.Where( s=>s.Dahan.Any )
			.Except(normalSpaces)
			.ToArray();
		// no dahan-only spaces, do normal
		if(dahanOnlySpaces.Length == 0)
			return await base.TargetsSpace( targetingPowerType , ctx, prompt, sourceCriteria, targetCriteria);

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
		return await this.Gateway.Decision( new Select.Space( "Target land with dahan", dahanOnlySpaces, Present.Always));
	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		var higestJungle = board.Spaces.Last( s=>s.IsJungle );

		this.Presence.Adjust( gs.Tokens[higestJungle],2 );
		this.Presence.Adjust( gs.Tokens[board[5]], 1 );
	}

}