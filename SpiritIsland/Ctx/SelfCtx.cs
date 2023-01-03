namespace SpiritIsland;

public class SelfCtx {

	#region private fields
	BoundPresence _presence; // lazy loaded
	#endregion

	public Spirit Self { get; }
	public GameState GameState { get; }
	public TerrainMapper TerrainMapper => ActionCtx.TerrainMapper;
	public UnitOfWork ActionCtx { get; }

	#region constructor

	public SelfCtx( Spirit self, GameState gameState, UnitOfWork actionId ) {

		// ! this bit checks if UnitOfWork and Terrain mapper is in sync
		//if( gameState.Island.Terrain != gameState.Island.Terrain_ForPower) {
		//	bool mapperIsForPower = mapper == gameState.Island.Terrain_ForPower;
		//	bool uowIsForPower = actionId.Category == ActionCategory.Spirit_Power;
		//	if( mapperIsForPower != uowIsForPower )	
		//		throw new Exception("out of sync");
		//}

		Self = self;
		GameState = gameState;
		ActionCtx = actionId;
	}

	protected SelfCtx(SelfCtx src) {
		Self = src.Self;
		GameState = src.GameState;
		ActionCtx = src.ActionCtx;
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
		=> Target(from).Tokens.MoveTo( token, to, ActionCtx );

	public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementCounts.Parse(elementString) );

	#endregion

	public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
		GameState.Fear.AddDirect( new FearArgs { 
			count = count, 
			FromDestroyedInvaders = false,
			space = null 
		} );
	}

	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption => Self.Gateway.Decision( originalDecision );


	public virtual TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );
	public TargetSpaceCtx Target( SpaceState ss ) => Target(ss.Space);

	public TargetSpiritCtx TargetSpirit( Spirit spirit ) => new TargetSpiritCtx( this, spirit );

	public SelfCtx NewSelf( Spirit spirit ) => spirit.BindSelf( GameState, ActionCtx );

	// Visually, selects the [presence] icon
	public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
		var space = await Decision( Select.DeployedPresence.All( prompt, Presence, Present.Always ) );
		return Target( space );
	}

	// Visually, selects the [space] which has presence.
	public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
		var space = await Decision( new Select.Space(prompt, Presence.SpaceStates, Present.Always ) );
		return Target( space );
	}

	public async Task FlipFearCard( IFearCard cardToFlip, bool activating = false ) {
		string label = activating ? "Activating Fear" : "Done";

		cardToFlip.Flipped = true;
		await Self.Select( label, new IOption[] { cardToFlip }, Present.Always );
		if( cardToFlip.Activation.HasValue )
			GameState.Log( new LogDebug( $"{cardToFlip.Activation.Value} => {cardToFlip.GetDescription( cardToFlip.Activation.Value )}" ) );
		else
			for(int i=1;i<=3;++i)
				GameState.Log( new LogDebug($"{i} => {cardToFlip.GetDescription(i)}") );
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

		var spaceCtx = await SelectSpace( "Remove invader from", spaceOptions );
		if(spaceCtx == null) return null;

		var options = spaceCtx.Tokens.OfAnyClass(removables);
		while(0<count && 0<options.Length) {
			var tokenToRemove = await spaceCtx.Self.Gateway.Decision( Select.TokenFrom1Space.TokenToRemove(spaceCtx.Space, count, options, Present.Always) );
			await spaceCtx.Tokens.Remove(tokenToRemove,1,ActionCtx);
			options = spaceCtx.Tokens.OfAnyClass( removables ); // next
			--count;
		}
		return spaceCtx?.Space;
	}

	#endregion

}