namespace SpiritIsland;

/// <summary>
/// Presents user with ability to select from many *REMOVABLE* tokens on many spaces.
/// Tracks selected tokens and excludes them from future selections.
/// </summary> <remarks> To include Non-Removable, call .NotRemoving() </remarks>
public class SourceSelector {

	#region constructors
	/// <summary> Tokens come from 1 space. </summary>
	public SourceSelector( Space sourceSpace ) {	_unfilteredSourceSpaces = [ sourceSpace ];	}
	/// <summary> Tokens come from 0..many spaces. </summary>
	public SourceSelector( IEnumerable<Space> sourceSpaces ) { _unfilteredSourceSpaces = sourceSpaces.ToArray(); }
	#endregion constructors

	public async IAsyncEnumerable<SpaceToken> GetEnumerator(
		Spirit spirit, 
		Func<PromptData,string> promptBuilder,
		Present present, 
		SpaceSpec? singleDestination = null,
		int? maxCount = null
	) {
		int index = 0;
		while(maxCount is null || index < maxCount.Value) {
			A.SpaceTokenDecision decision = BuildDecision( promptBuilder, present, singleDestination, index, maxCount );

			// Select Token
			SpaceToken? source = await spirit.Select( decision );
			if(source is null) break;

			await NotifyAsync( source );

			yield return source;
			++index;
		}
	}

	public A.SpaceTokenDecision BuildDecision( Func<PromptData, string> promptBuilder, Present present, SpaceSpec? singleDestination, 
		int index, int? maxCount // for the prompt
	) {
		SpaceToken[] options = GetSourceOptions();
		string prompt = promptBuilder( new PromptData( _quota, options, index, maxCount ) );
		return new A.SpaceTokenDecision( prompt, options, present )
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
		Dictionary<ITokenLocation,int> unused = [];
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
	public SourceSelector FilterSource( Func<Space, bool> filterSource ) {
		_filterSpace = filterSource;
		return this;
	}

	/// <summary>
	/// Normally selection based on IEntityClass.  Allows fine-graned selection based on specific IToken
	/// </summary>
	public SourceSelector FilterSpaceToken( Func<SpaceToken, bool> filterToken ) { _filterSpaceToken.Add(filterToken); return this; }

	#endregion public Config

	#region Event / Callback

	public SourceSelector Track( Action<ITokenLocation> onMoved ) {
		_onSelected.Add( ( x ) => { onMoved( x ); return Task.CompletedTask; } );
		return this;
	}

	public SourceSelector Track( Func<ITokenLocation, Task> onMoved ) {
		_onSelected.Add( onMoved );
		return this;
	}

	public async Task NotifyAsync( ITokenLocation selected ) {
		_quota.MarkTokenUsed( selected.Token );

		foreach(Func<ITokenLocation, Task> onSelected in _onSelected)
			await onSelected( selected );
	}

	readonly List<Func<ITokenLocation, Task>> _onSelected = [];

	#endregion

	#region protected methods

	public virtual SpaceToken[] GetSourceOptions() {
		var options = SourceSpaces
			.SelectMany( GetSourceOptionsOn1Space )
			.ToList();

		// User filter on SpaceTokens
		foreach(Func<SpaceToken, bool> f in _filterSpaceToken)
			options = options.Where(f).ToList();

		return [..options];
	}

	protected IEnumerable<SpaceToken> GetSourceOptionsOn1Space( Space sourceSpace ) 
		=> _quota.GetSourceOptionsOn1Space( sourceSpace );

	protected IEnumerable<Space> SourceSpaces
		=> _filterSpace == null ? _unfilteredSourceSpaces
		: _unfilteredSourceSpaces.Where( _filterSpace );

	#endregion

	public ITokenClass[] RemainingTypes => _quota.RemainingTypes;

	#region private 

	Quota _quota = new Quota();

	Func<Space, bool>? _filterSpace;
	readonly public Space[] _unfilteredSourceSpaces;

	readonly List<Func<SpaceToken, bool>> _filterSpaceToken = [];

	#endregion private
}


static public class SelectFrom {
	static public SourceSelector FromASingleLand( this SourceSelector ss ) {
		ILocation? source = null;
		ss
			.Track( spaceToken => source ??= spaceToken.Location )
			.FilterSource( space => source is null || space.Equals(source) );
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

public class PromptData( Quota quota, SpaceToken[] options, int index, int? maxCount = 0 ) {

	public readonly int Index = index;
	public readonly int? MaxCount = maxCount;
	public int RemainingCount => MaxCount.HasValue ? (MaxCount.Value - Index) : int.MaxValue;

	public string RemainingPartsStr => quota.RemainingTokenDescriptionOn( options.Select( st => st.Space ).Distinct().ToArray() );
}

