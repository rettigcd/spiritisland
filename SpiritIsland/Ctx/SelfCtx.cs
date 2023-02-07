namespace SpiritIsland;

public class SelfCtx {

	#region private fields
	BoundPresence _presence; // lazy loaded
	#endregion

	public Spirit Self { get; }
	public GameState GameState { get; }

	protected TerrainMapper TerrainMapper => _terrainMapper ??= UnitOfWork.Current.TerrainMapper;
	TerrainMapper _terrainMapper;

	#region constructor

	public SelfCtx( Spirit self, GameState gameState ) {
		Self = self;
		GameState = gameState;
	}

	protected SelfCtx(SelfCtx src) {
		Self = src.Self;
		GameState = src.GameState;
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
	public Task Move( IVisibleToken token, Space from, Space to )
		=> Target( from ).Tokens.MoveTo( token, to );


	public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementCounts.Parse(elementString) );

	#endregion

	public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
		GameState.Fear.AddDirect( new FearArgs( count) );
	}

	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption => Self.Gateway.Decision( originalDecision );


	public virtual TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );
	public TargetSpaceCtx Target( SpaceState ss ) => Target(ss.Space);

	public TargetSpiritCtx TargetSpirit( Spirit spirit ) => new TargetSpiritCtx( this, spirit );

	public SelfCtx NewSelf( Spirit spirit ) => spirit.BindSelf( GameState );

	// Visually, selects the [presence] icon
	public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
		var space = await Decision( Select.DeployedPresence.All( prompt, Presence, Present.Always ) );
		return Target( space );
	}

	// Visually, selects the [space] which has presence.
	public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
		var space = await Decision( new Select.Space(prompt, Presence.ActiveSpaceStates, Present.Always ) );
		return Target( space );
	}

	public virtual ActionableSpaceState TokensOn( Space space ) => GameState.Tokens[space].BindScope();

	public async Task FlipFearCard( IFearCard cardToFlip, bool activating = false ) {
		string label = activating ? "Activating Fear" : "Done";

		cardToFlip.Flipped = true;
		await Self.Select( label, new IOption[] { cardToFlip }, Present.Always );
		if( cardToFlip.ActivatedTerrorLevel.HasValue )
			GameState.Log( new Log.Debug( $"{cardToFlip.ActivatedTerrorLevel.Value} => {cardToFlip.GetDescription( cardToFlip.ActivatedTerrorLevel.Value )}" ) );
		else
			for(int i=1;i<=3;++i)
				GameState.Log( new Log.Debug($"{i} => {cardToFlip.GetDescription(i)}") );


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

}