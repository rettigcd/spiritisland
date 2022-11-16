namespace SpiritIsland;

public class SelfCtx {

	#region private fields
	BoundPresence _presence; // lazy loaded
	#endregion

	public Spirit Self { get; }
	public GameState GameState { get; }
	public TerrainMapper TerrainMapper { get; }
	public Guid CurrentActionId { get; }

	#region constructor

	public SelfCtx(Spirit self,GameState gameState, Cause cause, Guid actionId) {
		Self = self;
		GameState = gameState;
		TerrainMapper = cause == Cause.MyPowers
			? GameState.Island.Terrain_ForPower
			: GameState.Island.Terrain;
		CurrentActionId = actionId;
	}

	protected SelfCtx(SelfCtx src) {
		Self = src.Self;
		GameState = src.GameState;
		TerrainMapper = src.TerrainMapper;
		CurrentActionId = src.CurrentActionId;
	}

	#endregion constructor

	#region Presence

	public virtual BoundPresence Presence => _presence ??= new BoundPresence(this);

	#endregion

	#region convenience Read-Only methods

	/// <summary> 
	/// Simple wrapper around GameState.Tokens.Move 
	/// Used for Gathering / Pushing (and other stuff)
	/// </summary>
	public Task Move(Token token, Space from, Space to )
		=> Target(from).Tokens.MoveTo( token, to, CurrentActionId );

	public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementCounts.Parse(elementString) );

	#endregion

	public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
		GameState.Fear.AddDirect( new FearArgs { 
			count = count, 
			FromDestroyedInvaders = false,
			space = null 
		} );
	}

	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption => Self.Action.Decision( originalDecision );


	public virtual TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );

	public TargetSpiritCtx TargetSpirit( Spirit spirit ) => new TargetSpiritCtx( this, spirit );

	// Visually, selects the [presence] icon
	public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
		var space = await Decision( Select.DeployedPresence.All( prompt, Self,Present.Always ) );
		return Target( space );
	}

	// Visually, selects the [space] which has presence.
	public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
		var space = await Decision( new Select.Space(prompt,Self.Presence.Spaces, Present.Always ) );
		return Target( space );
	}

	#region Draw Cards

	public Task<DrawCardResult> Draw() => Self.Draw( GameState );
	public Task<DrawCardResult> DrawMinor() => Self.DrawMinor( GameState );
	public Task<DrawCardResult> DrawMajor( bool forgetCard, int numberToDraw = 4, int numberToKeep = 1 ) => Self.DrawMajor( GameState, forgetCard, numberToDraw, numberToKeep );


	#endregion

	#region Generic Select space / option

	public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options, Present present = Present.Always ) {
		var space = await Decision( new Select.Space( prompt, options, present ) );
		return space != null
			? Target( space )
			: null;
	}

	public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<TargetSpaceCtx> options ) {
		var lookup = options.ToDictionary(ctx=>ctx.Space,ctx=>ctx);
		if( !lookup.Any() ) return null; // ??? does this solve the Thriving Communities problem?
		var space = await Decision( new Select.Space( prompt, lookup.Keys, Present.Always ) );
		return space != null ? lookup[ space ] : null;
	}


	// overriden by Grinning Trickster's Lets See What Happens

	public Task SelectActionOption( params IExecuteOn<SelfCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<SelfCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
	public Task SelectAction_Optional( string prompt, params IExecuteOn<SelfCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

	virtual protected async Task SelectAction_Inner<T>( string prompt, IExecuteOn<T>[] options, Present present, T ctx ) {
		IExecuteOn<T>[] applicable = options
			.Where( opt => opt != null && opt.IsApplicable(ctx) )
			.ToArray();
		string text = await Self.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), present );
		if(text != null && text != TextOption.Done.Text) {
			var selectedOption = applicable.Single( a => a.Description == text );
			await selectedOption.Execute( ctx );
		}
	}

	public virtual Task Execute( IExecuteOn<SelfCtx> actionOption ) => actionOption.Execute(this);

	#endregion

	#region High level fear-specific decisions

	public async Task<Space> RemoveTokenFromOneSpace( IEnumerable<Space> spaceOptions, int count, params TokenClass[] removables ) {

		// !!!!!!!!  need to set action id


		var spaceCtx = await SelectSpace( "Remove invader from", spaceOptions );
		if(spaceCtx != null)
			while(count-->0)
				await spaceCtx.Invaders.Remove( removables );
		return spaceCtx?.Space;
	}

	#endregion

}