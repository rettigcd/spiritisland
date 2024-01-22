namespace SpiritIsland;

public class TargetSpaceCtx( Spirit self, Space target ) : IHaveASpirit {

	#region private fields
	InvaderBinding _invadersRO;
	BoundPresence_ForSpace _presence;
	SpaceState _tokens;
	#endregion

	public Space Space { get; } = target ?? throw new ArgumentNullException( nameof( target ) );

	#region constructors

	public Spirit Self { get; } = self;

	#endregion

	#region Parts from SelfCtx

	// ========== Parts from SelfCtx ===========

	public Task<bool> YouHave( string elementString ) => Self.YouHave( elementString );

	public Task<T> SelectAsync<T>( A.TypedDecision<T> originalDecision ) where T : class, IOption 
		=> Self.SelectAsync<T>( originalDecision );

	public TargetSpaceCtx Target( Space space ) => Self.Target( space );

	#endregion

	/// <summary> adds Target to Fear context </summary>
	public void AddFear( int count ) {
		Tokens.AddFear( count );
	}


	// overridden by Trickster to Select-All - !!! Could be put on Spirit to make easier to override, then we could seal this class.
	// !!! Check usages of Cmd.Pick1 and reroute through here for all Spirit Powers - so Trickster can do all.
	public virtual async Task SelectActionOption( params IActOn<TargetSpaceCtx>[] options ) {
		IActOn<TargetSpaceCtx>[] applicable = options
			.Where( opt => opt != null && opt.IsApplicable( this ) )
			.ToArray();

		string text = await Self.SelectText( "Select Power Option", applicable.Select( a => a.Description ).ToArray(), Present.AutoSelectSingle );

		if(text != null && text != TextOption.Done.Text) {
			var selectedOption = applicable.Single( a => a.Description == text );
			await selectedOption.ActAsync( this );
		}
	}


	public bool MatchesRavageCard => GameState.Current.InvaderDeck.Ravage.Cards.Any(c=>c.MatchesCard(Tokens));
	public bool MatchesBuildCard => GameState.Current.InvaderDeck.Build.Cards.Any(c=>c.MatchesCard(Tokens));

	public SpaceState Tokens => _tokens ??= Space.Tokens;

	#region Token Shortcuts
	public void Defend(int defend) => Tokens.Defend.Add(defend);
	public void Isolate() => Tokens.Isolate();

	public BeastBinding Beasts              => Tokens.Beasts;
	public TokenBinding Disease             => Tokens.Disease;
	public TokenBinding Wilds               => Tokens.Wilds;
	public TokenBinding Vitality            => Tokens.Vitality;
	public TokenBinding Badlands			=> Tokens.Badlands;
	public BlightTokenBinding Blight        => Tokens.Blight;
	public DahanBinding Dahan    => Tokens.Dahan;
	public Task AddDefault( HumanTokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Tokens.AddDefaultAsync( tokenClass, count, addReason );
	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.RemoveAsync( token, count, reason );

	#endregion

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	/// <returns>Destination</returns>
	public async Task<Space> MoveTokensToSingleLand( int max, TargetCriteria targetCriteria, params ITokenClass[] tokenClass ) {

		Space destination = null;

		await new TokenMover(Self,"Move",
			SourceSelector
				.AddGroup(max,tokenClass),
			new DestinationSelector( Range( targetCriteria ) )
				.Config( Distribute.ToASingleLand )
				.Track( to => destination = to.Space )
		).DoUpToN();

		return destination;
	}

	#region Push

	public Task PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, Human.Dahan );

	public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, Human.Dahan );

	/// <returns>Spaces pushed too.</returns>
	public async Task PushUpTo( int countToPush, params ITokenClass[] groups ) {
		await SourceSelector
			.AddGroup( countToPush, groups )
			.PushUpToN(Self);
	}

	public async Task<Space[]> Push( int countToPush, params ITokenClass[] groups ) {
		List<Space> destinations = [];
		await SourceSelector
			.AddGroup( countToPush, groups )
			.ConfigDestination( d=>d.Track( to => destinations.Add(to.Space) ) )
			.PushN( Self );
		return destinations.Distinct().ToArray();
	}

	public SourceSelector SourceSelector => Tokens.SourceSelector;

	#endregion Push

	#region Gather

	// Binds to Dahan
	public Task GatherUpToNDahan( int dahanToGather )
		=> GatherUpTo( dahanToGather, Human.Dahan );

	public Task GatherDahan( int countToGather )
		=> Gather( countToGather, Human.Dahan);

	public Task GatherUpTo( int countToGather, params ITokenClass[] groups )
		=> Gatherer.AddGroup(countToGather, groups).DoUpToN();

	public Task Gather( int countToGather, params ITokenClass[] groups )
		=> Gatherer.AddGroup(countToGather,groups).DoN();

	public TokenMover Gatherer => Tokens.Gather( Self );

	#endregion Gather

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<SpaceState> Adjacent => Tokens.Adjacent;
	public IEnumerable<TargetSpaceCtx> AdjacentCtxs => Adjacent.Select(adj=>Target(adj.Space)); // !!! ??? should we really spin up TargetSpaceCtx for each of these?

	public IEnumerable<SpaceState> Range( int range ) => Range( new TargetCriteria( range ) );

	/// <summary> Calculate Range using Power Range Calculator/Strategy. </summary>
	public IEnumerable<SpaceState> Range( TargetCriteria targetCriteria ) 
		=> Self.PowerRangeCalc.GetSpaceOptions(
			new SpaceState[] { Tokens },
			targetCriteria
		); // don't need to check .IsInPlay because TargetCriteria does that.

	#region Terrain

	TerrainMapper TerrainMapper => _terrainMapper ??= ActionScope.Current.TerrainMapper;
	TerrainMapper _terrainMapper;

	/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
	public bool IsOneOf(params Terrain[] terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool Is(Terrain terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool IsCoastal => TerrainMapper.IsCoastal( Tokens );
	public bool IsInland => TerrainMapper.IsInland( Tokens );
	#endregion

	public bool HasBlight => Blight.Any;

	public Task AddBlight(int delta, AddReason reason = AddReason.Added ) => Blight.AddAsync( delta, reason );

	/// <summary> Returns blight from the board to the blight card. </summary>
	public Task RemoveBlight(int count=1) => Blight.Remove( count );

	public int BlightOnSpace => Blight.Count;

	public bool HasInvaders => Tokens.HasInvaders();

	public void ModifyRavage( Action<RavageBehavior> action ) => action( Tokens.RavageBehavior );

	// The current targets power
	public InvaderBinding Invaders => _invadersRO ??= Tokens.Invaders;

	// Damage invaders in the current target space
	// This called both from powers and from Fear
	public async Task DamageInvaders( int originalDamage, params ITokenClass[] invaderTypesToDamage ) {
		if(originalDamage == 0) return;

		// Calculate Total Damage available
		CombinedDamage combinedDamage = Tokens.CombinedDamageFor_Invaders( originalDamage );
		int damageApplied = await Tokens.UserSelected_DamageInvadersAsync( Self, combinedDamage.Available, invaderTypesToDamage );
		combinedDamage.TrackDamageDone( damageApplied );
	}

	// For strifed Damage
	public async Task StrifedDamageOtherInvaders( int originalDamage, HumanToken damageSource, bool excludeSource ) {
		if(originalDamage == 0) return; // when no strifed invaders

		HumanToken damageSourceToExclude = excludeSource ? damageSource : null;
		HumanToken[] invadersToDamage() => Tokens.InvaderTokens().Where( t => t != damageSourceToExclude ).ToArray();

		// Calculate Total Damage available
		CombinedDamage combinedDamage = Tokens.CombinedDamageFor_Invaders( originalDamage );
		int damageApplied = await Tokens.UserSelected_ApplyDamageToSpecificTokenAsync( Self, combinedDamage.Available, damageSource, invadersToDamage );
		combinedDamage.TrackDamageDone( damageApplied );
	}

	public Task DamageEachInvader( int individualDamage ) => DamageEachInvader( individualDamage, Human.Invader);
	public async Task DamageEachInvader( int individualDamage, ITokenClass[] tokenClasses ) {
		await Invaders.ApplyDamageToEach( individualDamage, tokenClasses );

		var bonusDamage = Tokens.CombinedDamageFor_Invaders();
		int damageApplied = await Tokens.UserSelected_DamageInvadersAsync( Self, bonusDamage.Available, tokenClasses );
		bonusDamage.TrackDamageDone( damageApplied );
	}

	public async Task Apply1DamageToDifferentInvaders( int count ) {
		const int damagePerInvader = 1;

		// For Veil the Nights Hunt, badland damage can only be added to invaders already damaged. Might be different for other powers.

		// Find All Invaders
		var invaders = new List<IToken>();
		foreach(HumanToken token in Tokens.InvaderTokens())
			for(int i = 0; i < Tokens[token]; ++i)
				invaders.Add( token );

		// Limit # to select
		var damagedInvaders = new List<IToken>();
		count = System.Math.Min( count, invaders.Count );
		while(count-- > 0) {
			var st = await SelectAsync( An.Invader.ForIndividualDamage( damagePerInvader, invaders.On(Space) ) );
			if(st == null) break;
			HumanToken invader = st.Token.AsHuman();
			invaders.Remove( invader );
			var (_, damaged) = await Invaders.ApplyDamageTo1( damagePerInvader, invader );
			if(0 < damaged.RemainingHealth)
				damagedInvaders.Add( damaged );
		}

		var combined = Tokens.CombinedDamageFor_Invaders();
		int damageDone = await Tokens.UserSelected_ApplyDamageToSpecificTokensAsync( Self, damagedInvaders, combined.Available );
		combined.TrackDamageDone(damageDone); 
	}

	public async Task DamageDahan( int damage ) {
		if(damage == 0) return;

		var totalDamage = Tokens.CombinedDamageFor_Dahan( damage );
		int applied = await Dahan.ApplyDamage_Inefficiently( totalDamage.Available );
		totalDamage.TrackDamageDone( applied );
	}

	/// <summary> Incomporates bad lands </summary>
	public async Task Apply1DamageToEachDahan() {

		await Dahan.Apply1DamageToAll();
		var moreDamage = Tokens.CombinedDamageFor_Dahan();
		int applied = await Dahan.ApplyDamage_Inefficiently( moreDamage.Available );
		moreDamage.TrackDamageDone( applied );
	}

	public Task AddStrife( params HumanTokenClass[] groups ) => AddStrife(1,groups);

	/// <param name="groups">Option: if null/empty, no filtering</param>
	public async Task AddStrife( int strifeCount, params HumanTokenClass[] groups ) {
		if( groups.Length == 0) groups = Human.Invader;
		await SourceSelector.AddGroup( strifeCount, groups ).StrifeAll(Self);
	}

	public Task RemoveInvader( ITokenClass group, RemoveReason reason = RemoveReason.Removed ) => Invaders.RemoveLeastDesirable( reason, group );

	#region presence

	public BoundPresence_ForSpace Presence => _presence ??= new BoundPresence_ForSpace(this);


	// ! See base class for more Presence options

	public bool IsSelfSacredSite => Self.Presence.IsSacredSite(Tokens);

	public int PresenceCount => Self.Presence.CountOn( Tokens );

	/// <summary> Spirit has presence on this space. </summary>
	public bool IsPresent => Self.Presence.IsOn( Tokens );

	public async Task PlacePresenceHere() {
		var from = await Self.SelectSourcePresence();
		await from.MoveToAsync(Tokens);
	}

	#endregion

	/// <remarks> Simnple Helper - has access to: Spirit, SpaceState/Adjacent, Target() </remarks>
	public async Task<TargetSpaceCtx> SelectAdjacentLandAsync( string prompt ) {
		var space = await SelectAsync( new A.Space( prompt, Tokens.Adjacent, Present.Always ) );
		return space != null ? Target( space )
			: null;
	}

	/// <remarks>This could be on GameState but everywhere it is used has access to TargetSpaceCtx and it is more convenient here.</remarks>
	public ITokenClass[] AllPresenceTokens => GameState.Current.Spirits
		.SelectMany(s=>s.Presence.TokensDeployedOn( Space ) )
		.Select(x=>x.Class)
		.ToArray();
}