namespace SpiritIsland;

/// <summary>
/// Presents user with ability to select from many *REMOVABLE* tokens on many spaces.
/// Tracks selected tokens and excludes them from future selections.
/// </summary> <remarks> To include Non-Removable, call .IncludeNonRemovable() </remarks>
public class SourceSelector {

	#region constructors
	public SourceSelector( params SpaceState[] sourceSpaces ) {	_unfilteredSourceSpaces = sourceSpaces;	}
	public SourceSelector( IEnumerable<SpaceState> sourceSpaces ) { _unfilteredSourceSpaces = sourceSpaces.ToArray(); }
	#endregion constructors

	/// <summary>
	/// Select a token from the remaining group candidates.  May be called many times in succession.
	/// </summary>
	/// <param name="spirit">spirit making the sele</param>
	/// <param name="actionPromptPrefix">Move, Push, Gather, Bring</param>
	/// <param name="present">Required or not</param>
	/// <param name="singleDestination">Draws arrows to destination if there is only one.</param>
	public async Task<SpaceToken> GetSource( Spirit spirit, string actionPromptPrefix, Present present, Space singleDestination = null ) {
		SpaceToken[] options = await GetSourceOptions();
		SpaceState[] sourceSpaces = options.Select( st => st.Space ).Distinct().Tokens().ToArray();
		string remaining = _quota.RemainingTokenDescriptionOn( sourceSpaces );
		string prompt = $"{actionPromptPrefix} ({remaining})";

		// Select Token
		SpaceToken source = await spirit.Select( new A.SpaceToken( prompt, options, present ).PointArrowTo( singleDestination ) );
		if(source != null){
			_quota.MarkTokenUsed( source.Token );
			await NotifyAsync( source );
		}
		return source;
	}

	#region public Config

	/// <summary> Specifies 1 or more tokens that may be selected - must be called at least once for GetSource to present any results.</summary>
	public SourceSelector AddGroup( int count, params IEntityClass[] classes ) { _quota.AddGroup( count, classes ); return this; }

	public SourceSelector AddAll( params IEntityClass[] classes ) { _quota.AddAll( classes ); return this; }

	public SourceSelector UseQuota( Quota quota ) { _quota = quota; return this; }

	public SourceSelector IncludeNonRemovable(){ _includeNonRemovable = true; return this; }

	/// <summary> Dynamically filter sources. - when sources may change over time. </summary>
	public SourceSelector FilterSource( Func<SpaceState, bool> filterSource ) {
		_filterSource = filterSource;
		return this;
	}

	#endregion public Config

	#region Event / Callback

	public SourceSelector Track( Action<SpaceToken> onMoved ) {
		_onSelected.Add( ( x ) => { onMoved( x ); return Task.CompletedTask; } );
		return this;
	}

	public SourceSelector Track( Func<SpaceToken, Task> onMoved ) {
		_onSelected.Add( onMoved );
		return this;
	}

	async Task NotifyAsync( SpaceToken selected ) {
		foreach(var onSelected in _onSelected)
			await onSelected( selected );
	}

	readonly List<Func<SpaceToken, Task>> _onSelected = new();

	#endregion

	#region protected methods

	protected virtual async Task<SpaceToken[]> GetSourceOptions() {
		var options = new List<SpaceToken>();
		foreach(SpaceState sourceSpaceState in SourceSpaces) {
			options.AddRange(
				await GetSourceOptionsOn1Space( sourceSpaceState )
			);
		}
		return options.ToArray();
	}

	protected async Task<IEnumerable<SpaceToken>> GetSourceOptionsOn1Space( SpaceState sourceSpaceState ) {
		IToken[] tokens = _includeNonRemovable
			? sourceSpaceState.OfAnyClass(_quota.RemainingTypes).Cast<IToken>().ToArray()
			: await sourceSpaceState.RemovableOfAnyClass( RemoveReason.MovedFrom, _quota.RemainingTypes );
		return tokens.On( sourceSpaceState.Space );
	}

	protected IEnumerable<SpaceState> SourceSpaces
		=> _filterSource == null ? _unfilteredSourceSpaces
		: _unfilteredSourceSpaces.Where( _filterSource );

	#endregion

	#region private 

	public IEntityClass[] RemainingTypes => _quota.RemainingTypes;
	Quota _quota = new Quota();

	Func<SpaceState, bool> _filterSource;
	readonly public SpaceState[] _unfilteredSourceSpaces;

	bool _includeNonRemovable = false;

	#endregion private
}
