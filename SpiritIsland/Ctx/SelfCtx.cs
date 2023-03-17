namespace SpiritIsland;

public class SelfCtx {

	public Spirit Self { get; }
	public GameState GameState => _gameState ??= GameState.Current;
	GameState _gameState;

	protected TerrainMapper TerrainMapper => _terrainMapper ??= ActionScope.Current.TerrainMapper;
	TerrainMapper _terrainMapper;

	#region constructor

	public SelfCtx( Spirit self ) {
		Self = self;
	}

	protected SelfCtx(SelfCtx src) {
		Self = src.Self;
	}

	#endregion constructor


	#region convenience Read-Only methods

	/// <summary> 
	/// Simple wrapper around GameState.Tokens.Move 
	/// Used for Gathering / Pushing (and other stuff)
	/// </summary>
	public Task Move( IToken token, Space from, Space to )
		=> Target( from ).Tokens.MoveTo( token, to );


	public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementCounts.Parse(elementString) );

	#endregion

	public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
		GameState.Fear.AddDirect( new FearArgs( count) );
	}

	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption => Self.Gateway.Decision( originalDecision );

	public TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space ); // Trickster
	public TargetSpaceCtx Target( SpaceState ss ) => Target(ss.Space);

	public TargetSpiritCtx TargetSpirit( Spirit spirit ) => new TargetSpiritCtx( this, spirit );

	// Visually, selects the [presence] icon
	public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
		var space = await Decision( Select.DeployedPresence.All( prompt, Self.Presence, Present.Always ) );
		return Target( space );
	}

	// Visually, selects the [space] which has presence.
	public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
		var space = await Decision( new Select.ASpace(prompt, Self.Presence.Spaces.Tokens(), Present.Always ) );
		return Target( space );
	}

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

	public Task<DrawCardResult> Draw() => Self.Draw();
	public Task<DrawCardResult> DrawMinor() => Self.DrawMinor();
	public Task<DrawCardResult> DrawMajor( bool forgetCard, int numberToDraw = 4, int numberToKeep = 1 ) => Self.DrawMajor( forgetCard, numberToDraw, numberToKeep );


	#endregion

	#region Generic Select space / option

	public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options, Present present = Present.Always ) {
		var space = await Decision( new Select.ASpace( prompt, options, present ) );
		return space != null
			? Target( space )
			: null;
	}

	// overriden by Grinning Trickster's Lets See What Happens

	public Task SelectActionOption( params IExecuteOn<SelfCtx>[] options ) 
		=> SelectAction_Inner1( "Select Power Option", options, Present.AutoSelectSingle );

	public Task SelectAction_Optional( string prompt, params IExecuteOn<SelfCtx>[] options )
		=> SelectAction_Inner1( prompt, options, Present.Done );

	protected async Task SelectAction_Inner1(
		string prompt,
		IExecuteOn<SelfCtx>[] options,
		Present present
	) {
		IExecuteOn<SelfCtx>[] applicable = options
			.Where( opt => opt != null && opt.IsApplicable( this ) )
			.ToArray();

		string text = await Self.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), present );

		if(text != null && text != TextOption.Done.Text) {
			var selectedOption = applicable.Single( a => a.Description == text );
			await selectedOption.Execute( this );
		}
	}

	#endregion

}