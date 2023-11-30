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
		while(maxCount is null || index < maxCount.Value){
			// SpaceToken x = await GetSource( spirit, prompt, present, destination );
			SpaceToken[] options = await GetSourceOptions();

			string prompt = promptBuilder(new PromptData(_quota,options,index,maxCount));

			// Select Token
			SpaceToken source = await spirit.Select( new A.SpaceToken( prompt, options, present ).PointArrowTo( singleDestination ) );
			if(source != null) {
				_quota.MarkTokenUsed( source.Token );
				await NotifyAsync( source );
			}

			if(source == null) break;
			yield return source;
			++index;
		}
	}

	#region public Config

//	public SourceSelector Config( Func<SourceSelector,SourceSelector> configuration ) { configuration(this); return this;}
	public SourceSelector Config( Action<SourceSelector> configuration ) { configuration(this); return this;}

	/// <summary>
	/// Tracks starting invaders and only allows each to be selected once.
	/// </summary>
	/// <remarks>Used primarily for damage or when not removing tokens.</remarks>
	public SourceSelector ConfigOnlySelectEachOnce(){
		// Tokens will still be where they started, so we need to
		// manually track how many have been used and
		// not allow selection when used up
		CountDictionary<SpaceToken> selected = new();
		Track( st => selected[st]++ );
		FilterSpaceToken( st => selected[st] < st.Count);

		_removeReason = RemoveReason.None; 
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

		// User filter on SpaceTokens
		foreach(Func<SpaceToken, bool> f in _filterSpaceToken)
			options = options.Where(f).ToList();

		return options.ToArray();
	}

	protected Task<IEnumerable<SpaceToken>> GetSourceOptionsOn1Space( SpaceState sourceSpaceState ) 
		=> _quota.GetSourceOptionsOn1Space( sourceSpaceState, _removeReason );

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

	RemoveReason _removeReason = RemoveReason.MovedFrom;

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
	static public Func<PromptData,string> RemainingParts(string prefix) => (x) => $"{prefix} ({x.RemainingPartsStr})";
	static public Func<PromptData,string> RemainingCount(string prefix) => (x) => $"{prefix} ({x.RemainingCount} remaining)";
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

