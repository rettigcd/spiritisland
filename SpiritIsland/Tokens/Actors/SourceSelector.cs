namespace SpiritIsland;

/// <summary>
/// Presents user with ability to select from many *REMOVABLE* tokens on many spaces.
/// Tracks selected tokens and excludes them from future selections.
/// </summary> <remarks> To include Non-Removable, call .NotRemoving() </remarks>
public class SourceSelector {

	#region constructors
	/// <summary> Tokens come from 1 space. </summary>
	public SourceSelector( SpaceState sourceSpace ) {	_unfilteredSourceSpaces = new[]{ sourceSpace };	}
	/// <summary> Tokens come from 0..many spaces. </summary>
	public SourceSelector( IEnumerable<SpaceState> sourceSpaces ) { _unfilteredSourceSpaces = sourceSpaces.ToArray(); }
	#endregion constructors

	public async IAsyncEnumerable<SpaceToken> GetEnumerator(
		Spirit spirit, 
		Func<PromptData,string> promptBuilder,
		Present present, 
		Space singleDestination=null,
		int? maxCount = null
	) {
		int index = 0;
		while(maxCount is null || index < maxCount.Value) {
			A.SpaceToken decision = BuildDecision( promptBuilder, present, singleDestination, index, maxCount );

			// Select Token
			SpaceToken source = await spirit.SelectAsync( decision );
			if(source == null) break;

			await NotifyAsync( source );

			yield return source;
			++index;
		}
	}

	public A.SpaceToken BuildDecision( Func<PromptData, string> promptBuilder, Present present, Space singleDestination, 
		int index, int? maxCount // for the prompt
	) {
		SpaceToken[] options = GetSourceOptions();
		string prompt = promptBuilder( new PromptData( _quota, options, index, maxCount ) );
		return new A.SpaceToken( prompt, options, present )
			.PointArrowTo( singleDestination );
	}

	#region public Config

	//	public SourceSelector Config( Func<SourceSelector,SourceSelector> configuration ) { configuration(this); return this;}
	public SourceSelector Config( Action<SourceSelector> configuration ) { configuration(this); return this;}

	/// <summary>
	/// Tracks starting invaders and only allows each to be selected once.
	/// </summary>
	/// <remarks>
	/// Used primarily for damage or when not removing tokens.
	/// </remarks>
	public SourceSelector ConfigOnlySelectEachOnce(){
		// Tokens will still be where they started, so we need to
		// manually track how many have been used and
		// not allow selection when used up
		Dictionary<SpaceToken,int> unused = new();
		Track( st => {
			// first time - init total
			if(!unused.ContainsKey(st)) unused.Add(st,st.Count);
			// remove
			--unused[st];
	 	} );
		FilterSpaceToken( st => !unused.ContainsKey(st) || unused[st] != 0);

		return this;
	}


	/// <summary> Specifies 1 or more tokens that may be selected - must be called at least once for GetSource to present any results.</summary>
	public SourceSelector AddGroup( int count, params ITokenClass[] classes ) { _quota.AddGroup( count, classes ); return this; }

	public SourceSelector AddAll( params ITokenClass[] classes ) { _quota.AddAll( classes ); return this; }

	public SourceSelector UseQuota( Quota quota ) { _quota = quota; return this; }

	/// <summary> Dynamically filter sources. - when sources may change over time. </summary>
	public SourceSelector FilterSource( Func<SpaceState, bool> filterSource ) {
		_filterSpace = filterSource;
		return this;
	}

	/// <summary>
	/// Normally selection based on IEntityClass.  Allows fine-graned selection based on specific IToken
	/// </summary>
	public SourceSelector FilterSpaceToken( Func<SpaceToken, bool> filterToken ) { _filterSpaceToken.Add(filterToken); return this; }

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

	public async Task NotifyAsync( SpaceToken selected ) {
		_quota.MarkTokenUsed( selected.Token );

		foreach(Func<SpaceToken, Task> onSelected in _onSelected)
			await onSelected( selected );
	}

	readonly List<Func<SpaceToken, Task>> _onSelected = new();

	#endregion

	#region protected methods

	public virtual SpaceToken[] GetSourceOptions() {
		//var options = new List<SpaceToken>();
		//foreach(SpaceState sourceSpaceState in SourceSpaces)
		//	options.AddRange( GetSourceOptionsOn1Space( sourceSpaceState ) );
		var optionz = SourceSpaces
			.SelectMany( GetSourceOptionsOn1Space )
			.ToList();

		// User filter on SpaceTokens
		foreach(Func<SpaceToken, bool> f in _filterSpaceToken)
			optionz = optionz.Where(f).ToList();

		return optionz.ToArray();
	}

	protected IEnumerable<SpaceToken> GetSourceOptionsOn1Space( SpaceState sourceSpaceState ) 
		=> _quota.GetSourceOptionsOn1Space( sourceSpaceState );

	protected IEnumerable<SpaceState> SourceSpaces
		=> _filterSpace == null ? _unfilteredSourceSpaces
		: _unfilteredSourceSpaces.Where( _filterSpace );

	#endregion

	public ITokenClass[] RemainingTypes => _quota.RemainingTypes;

	#region private 

	Quota _quota = new Quota();

	Func<SpaceState, bool> _filterSpace;
	readonly public SpaceState[] _unfilteredSourceSpaces;

	readonly List<Func<SpaceToken, bool>> _filterSpaceToken = new();

	#endregion private
}


static public class SelectFrom {
	static public SourceSelector FromASingleLand( this SourceSelector ss ) {
		Space source = null;
		ss
			.Track( spaceToken => source ??= spaceToken.Space )
			.FilterSource( spaceState => source is null || spaceState.Space == source );
		return ss;
	}
}

public class Prompt {
	/// <summary> Does not require MaxCount  </summary>
	static public Func<PromptData,string> RemainingParts(string prefix) => (x) => $"{prefix} ({x.RemainingPartsStr})";
	/// <summary> Requires non-null MaxCount  </summary>
	static public Func<PromptData,string> RemainingCount(string prefix) => (x) => $"{prefix} ({x.RemainingCount} remaining)";
	/// <summary> Requires non-null MaxCount  </summary>
	static public Func<PromptData,string> XofY(string prefix) => (x) => $"{prefix} ({x.Index+1} of {x.MaxCount})";
}

public class PromptData {

	readonly Quota _quota;
	readonly SpaceToken[] _options;
	public readonly int Index;
	public readonly int? MaxCount;
	public int RemainingCount => MaxCount.Value - Index;
	public PromptData(Quota quota, SpaceToken[] options, int index, int? maxCount = 0) {
		_quota = quota;
		_options = options;
		Index = index;
		MaxCount = maxCount;
	}
	public string RemainingPartsStr => _quota.RemainingTokenDescriptionOn( _options.Select( st => st.Space ).Distinct().Tokens().ToArray() );
}

