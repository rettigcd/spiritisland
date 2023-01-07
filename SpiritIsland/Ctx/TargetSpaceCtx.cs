namespace SpiritIsland;

public class TargetSpaceCtx : SelfCtx {

	#region private fields
	DamagePool _badlandDamage;
	DamagePool _bonusDamageFromSpirit;
	InvaderBinding _invadersRO;
	BoundPresence_ForSpace _presence;
	ActionableSpaceState _tokens;
	#endregion

	public Space Space { get; }

	#region constructors

	// Called:
	//		from SelfCtx (to target a space)
	//		for derived types
	public TargetSpaceCtx( SelfCtx ctx, Space target )
		:base( ctx )
	{
		Space = target ?? throw new ArgumentNullException(nameof(target));
	}

	#endregion


	public Task SelectActionOption( params IExecuteOn<TargetSpaceCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<TargetSpaceCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );

	public bool MatchesRavageCard => GameState.InvaderDeck.Ravage.Cards.Any(c=>c.MatchesCard(Tokens));
	public bool MatchesBuildCard => GameState.InvaderDeck.Build.Cards.Any(c=>c.MatchesCard(Tokens));

	public ActionableSpaceState Tokens => _tokens ??= TokensOn(Space);

	#region Token Shortcuts
	public void Defend(int defend) => Tokens.Defend.Add(defend);
	public void Isolate() {
		Tokens.Init(TokenType.Isolate,1); // not a real token
		GameState.TimePasses_ThisRound.Push( (gs)=>{ 
			Tokens.Init(TokenType.Isolate,0);
			return Task.CompletedTask; 
		} ); // !! could just sweep entire board instead...
	}

	public TokenBinding Beasts               => Tokens.Beasts.Bind( ActionScope );
	public TokenBinding Disease              => Tokens.Disease.Bind( ActionScope );
	public TokenBinding Wilds                => Tokens.Wilds.Bind( ActionScope );
	public virtual TokenBinding Badlands     => Tokens.Badlands.Bind( ActionScope );
	public virtual HealthTokenClassBinding Dahan   => Tokens.Dahan.Bind( ActionScope ); // Powers that interact with dahan, MUST go through this property 
	public virtual BlightTokenBinding Blight => Tokens.Blight.Bind( ActionScope );
	public Task AddDefault( HealthTokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Tokens.AddDefault( tokenClass, count, addReason );
	public Task Remove( Token token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, reason );
	public Task Destroy( Token token, int count ) 
		=> Tokens.Destroy( token, count );

	#endregion

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	/// <returns>Destination</returns>
	public async Task<Space> MoveTokensOut( int max, TargetCriteria targetCriteria, params TokenClass[] tokenClass ) {

		if(!Tokens.HasAny( tokenClass )) return null;

		// Select Destination
		var destinationOptions = Range( targetCriteria );
		Space destination = await Decision( Select.Space.MoveToken( Space, destinationOptions, Present.Done, null ) );

		Token[] tokenOptions = Tokens.OfAnyClass( tokenClass );
		int remaining = Math.Min( Tokens.SumAny(tokenClass), max );
		while(tokenOptions.Length > 0 && remaining > 0 ) {
			// Select Token and move
			var source = await Decision( Select.TokenFrom1Space.TokenToMove( Space, remaining, tokenOptions, Present.Done ) );
			if(source == null) break;
			await Move( source, Space, destination );

			// Next
			--remaining;
			tokenOptions = Tokens.OfAnyClass( tokenClass );
		}

		return destination;

	}

	#region Push

	public Task<Space[]> PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, TokenType.Dahan );

	public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, TokenType.Dahan );

	// overriden by Grinning Tricksters Let's See what happens
	/// <returns>Spaces pushed too.</returns>
	public virtual Task<Space[]> PushUpTo( int countToPush, params TokenClass[] groups )
		=> Pusher.AddGroup( countToPush, groups ).MoveUpToN();

	public Task<Space[]> Push( int countToPush, params TokenClass[] groups )
		=> Pusher.AddGroup( countToPush, groups ).MoveN();

	public virtual TokenPusher Pusher => new TokenPusher( this );

	#endregion Push

	#region Gather

	// Binds to Dahan
	public Task GatherUpToNDahan( int dahanToGather )
		=> this.GatherUpTo( dahanToGather, TokenType.Dahan );

	public Task GatherDahan( int countToGather )
		=> this.Gather( countToGather, TokenType.Dahan);

	// overriden by Grinning Tricketsrs 'Let's see what happens'
	public virtual Task GatherUpTo( int countToGather, params TokenClass[] groups )
		=> Gatherer.AddGroup(countToGather, groups).GatherUpToN();

	public Task Gather( int countToGather, params TokenClass[] groups )
		=> Gatherer.AddGroup(countToGather,groups).GatherN();

	public virtual TokenGatherer Gatherer => new TokenGatherer( this );

	#endregion Gather

	public Task MoveTo( Token token, Space to ) => Tokens.MoveTo( token, to );

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<SpaceState> Adjacent => Tokens.Adjacent.Where( TerrainMapper.IsInPlay );
	public IEnumerable<TargetSpaceCtx> AdjacentCtxs => Adjacent.Select(Target);

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	// !!! This is Range-From-Here.  Compare it to BoundPresence.FindSpacesWithinRange & Spirit.FindSpacesWithinRange which is Range-From-Presence
	public IEnumerable<SpaceState> Range( TargetCriteria targetCriteria ) => Self.PowerRangeCalc.GetTargetOptionsFromKnownSource(
		new SpaceState[] { Tokens },
		targetCriteria
	)
		.Where( TerrainMapper.IsInPlay ); // !!! is this necessary?  Does the RangeCalc already check this?
	public IEnumerable<SpaceState> Range( int range ) => Range( TerrainMapper.Specify(range) );

	#region Terrain

	/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
	public bool IsOneOf(params Terrain[] terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool Is(Terrain terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool IsCoastal => TerrainMapper.IsCoastal( Tokens );
	public bool IsInland => TerrainMapper.IsInland( Tokens );
	public bool IsInPlay => TerrainMapper.IsInPlay( Tokens );
	#endregion

	public bool HasBlight => Blight.Any;

	public Task AddBlight(int delta, AddReason reason = AddReason.Added ) => Blight.Add( delta, reason );

	/// <summary> Returns blight from the board to the blight card. </summary>
	public Task RemoveBlight(int count=1) => Blight.Remove( count, RemoveReason.ReturnedToCard );

	public int BlightOnSpace => Blight.Count;

	public bool HasInvaders => Tokens.HasInvaders();

	public void ModifyRavage( Action<RavageBehavior> action ) => GameState.ModifyRavage(Space,action);

	// The current targets power
	public InvaderBinding Invaders => _invadersRO ??= GetInvaders();

	protected virtual InvaderBinding GetInvaders() => new InvaderBinding( Tokens );

	public void SkipAllInvaderActions(string label) => Tokens.SkipAllInvaderActions( label );

	public void Skip1Build( string label ) => Tokens.Skip1Build( label );

	public void Skip1Ravage( string label ) => Tokens.Skip1Ravage( label );


	// Damage invaders in the current target space
	// This called both from powers and from Fear
	public async Task DamageInvaders( int originalDamage, params TokenClass[] allowedTypes ) {

		// Calculate Total Damage available
		int sumAvailableDamage = originalDamage;
		sumAvailableDamage += BonusDamage.Remaining;
		if(0 < originalDamage)
			sumAvailableDamage += BadlandDamage.Remaining;

		// Apply Damage
		int damageApplied = await Invaders.UserSelectedDamage( sumAvailableDamage, Self, allowedTypes );
		int poolDamageToAccountFor = damageApplied - originalDamage;

		// Remove bonus damage from damage pools
		poolDamageToAccountFor -= BadlandDamage.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= BonusDamage.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}

	// For strifed Damage
	// !!! ??? Can this be combined with DamageInvaders() to remove duplication?
	public async Task StrifedDamageOtherInvaders( int originalDamage, HealthToken damageSource, bool excludeSource ) {

		HealthToken damageSourceToExclude = excludeSource ? damageSource : null;
		HealthToken[] invadersToDamage() => Tokens.InvaderTokens()
			.Where( t => t != damageSourceToExclude )
			.ToArray();

		// Calculate Total Damage available
		int sumAvailableDamage = originalDamage;
		sumAvailableDamage += BonusDamage.Remaining;
		if(0 < originalDamage)
			sumAvailableDamage += BadlandDamage.Remaining;

		// Apply Damage
		int damageApplied = await Invaders.UserSelected_ApplyDamageToSpecificToken( sumAvailableDamage, Self, damageSource, invadersToDamage );
		int poolDamageToAccountFor = damageApplied - originalDamage;

		// Remove bonus damage from damage pools
		poolDamageToAccountFor -= BadlandDamage.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= BonusDamage.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}

	DamagePool BonusDamage => _bonusDamageFromSpirit ??= new DamagePool( Self.BonusDamage );

	DamagePool BadlandDamage => _badlandDamage ??= new DamagePool( Badlands.Count );

	class DamagePool {

		public DamagePool( int init ) { remaining = init; }

		public int ReducePoolDamage( int poolDamageToAccountFor ) {
			int damageFromBadlandPool = Math.Min( remaining, poolDamageToAccountFor );
			remaining -= damageFromBadlandPool;
			return damageFromBadlandPool;
		}

		int remaining;
		public int Remaining => remaining;
	}

	public async Task DamageEachInvader( int individualDamage, params TokenClass[] generic ) {
		await Invaders.ApplyDamageToEach( individualDamage, generic );
		await Invaders.UserSelectedDamage( Badlands.Count, Self,generic ); // !!! use badland DamagePool
	}

	public async Task Apply1DamageToDifferentInvaders( int count ) {
		const int damagePerInvader = 1;

		// !!! Add Damage Pool (badlands / Flame's Furry) to this.
		// For Veil the Nights Hunt, badland damage can only be added to invaders already damaged. Might be different for other powers.

		// Find All Invaders
		var invaders = new List<Token>();
		foreach(var token in Tokens.InvaderTokens())
			for(int i = 0; i < Tokens[token]; ++i)
				invaders.Add( token );

		// Limit # to select
		var damagedInvaders = new List<Token>();
		count = System.Math.Min( count, invaders.Count );
		while(count-- > 0) {
			var invader = (HealthToken)await Decision( Select.Invader.ForIndividualDamage( damagePerInvader, Space, invaders ) );
			if(invader == null) break;
			invaders.Remove( invader );
			var (_, damaged) = await Invaders.ApplyDamageTo1( damagePerInvader, invader );
			if(damaged.RemainingHealth > 0)
				damagedInvaders.Add( damaged );
		}

		await ApplyDamageToSpecificTokens( damagedInvaders, Badlands.Count );
	}

	async Task ApplyDamageToSpecificTokens( List<Token> invaders, int additionalTotalDamage ) {
		while(additionalTotalDamage > 0) {
			var invader = (HealthToken)await Decision( Select.Invader.ForBadlandDamage(additionalTotalDamage,Space,invaders) );
			if(invader == null) break;
			int index = invaders.IndexOf( invader );
			var (_, moreDamaged) = await Invaders.ApplyDamageTo1( 1, invader );
			if(moreDamaged.RemainingHealth > 0)
				invaders[index] = moreDamaged;
			else
				invaders.RemoveAt( index );
		}
	}


	public async Task DamageDahan( int damage ) {
		if(damage == 0) return;

		// !!! This is not correct, if card has multiple Damage-Dahans, adds badland multiple times.
		damage += Badlands.Count;

		// and damage to dahan.
		await Dahan.ApplyDamage_Inefficiently( damage );
	}

	/// <summary> Incomporates bad lands </summary>
	public async Task Apply1DamageToEachDahan() {
		await Dahan.Apply1DamageToAll();
		await Dahan.ApplyDamage_Inefficiently( Badlands.Count);
	}

	#region Add Strife

	/// <param name="groups">Option: if null/empty, no filtering</param>
	public virtual async Task AddStrife( params TokenClass[] groups ) {
		var invader = (HealthToken) await Decision( Select.Invader.ForStrife( Tokens, groups ) );
		if(invader == null) return;
		await Tokens.AddStrifeTo( invader );
	}

	public Task AddStrifeTo( HealthToken invader, int count = 1 ) {
		return Tokens.AddStrifeTo( invader, count );
	}


	#endregion

	public Task RemoveInvader( TokenClass group ) => Invaders.RemoveLeastDesirable( group );

	/// <summary> adds Target to Fear context </summary>
	public override void AddFear( int count ) { 
		GameState.Fear.AddDirect( new FearArgs( count ) { space = Space } );
	}

	#region presence

	public new BoundPresence_ForSpace Presence => _presence ??= new BoundPresence_ForSpace(this);


	// ! See base class for more Presence options

	public bool IsSelfSacredSite => Presence.IsSacredSite(Space);

	public int PresenceCount => Self.Presence.CountOn(Tokens);

	public bool IsPresent => Self.Presence.IsOn( Tokens );

	public async Task PlacePresenceHere() {
		var from = await Presence.SelectSource();
		await Self.Presence.Place( from, Space, GameState, ActionScope ); //!! use Bounded presence instead
	}

	#endregion

	/// <param name="tokenToAdd">IF we are adding a token, supply it so UI can better pick target sapce</param>
	public async Task<TargetSpaceCtx> SelectAdjacentLand( string prompt, Token tokenToAdd = null, Func<TargetSpaceCtx, bool> filter = null ) {
		var options = Tokens.Adjacent;
		if(filter != null)
			options = options.Where( s => filter( Target( s.Space ) ) );
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always, tokenToAdd ) ); // !! could let caller pass in direction if appropriate
		return space != null ? Target( space )
			: null;
	}

	public async Task<TargetSpaceCtx> SelectAdjacentLandOrSelf( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
		List<SpaceState> options = Tokens.Adjacent.ToList();
		options.Add(Tokens);
		if(filter != null)
			options = options.Where( s => filter( Target( s.Space ) ) ).ToList();
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always, null ) );
		return space != null ? Target( space )
			: null;
	}

	/// <remarks>This could be on GameState but everywhere it is used has access to TargetSpaceCtx and it is more convenient here.</remarks>
	public TokenClass[] AllPresenceTokens => GameState.Spirits
		.Select( s => s.Presence.Token )
		.ToArray();

}