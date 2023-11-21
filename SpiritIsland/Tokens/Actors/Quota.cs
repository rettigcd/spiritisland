namespace SpiritIsland;

/// <summary>
/// Tracks how many tokens of a particular type are allowed to do something
/// </summary>
public class Quota {

	public void MarkTokenUsed( IToken token ) {
		var match = _sharedGroupCounts.First( q => q.Contains( token.Class ) );
		match.UsedOne();
		if(!match.HasMore)
			_sharedGroupCounts.Remove( match );

		_remainingTypes = null; // recalc next time neede
	}

	public ITokenClass[] RemainingTypes => _remainingTypes ??= CalcRemainingTypes();

	public string RemainingTokenDescriptionOn( SpaceState[] sourceSpaces ) => _beVerbose 
		? _sharedGroupCounts.Select( x => x.VerboseString( sourceSpaces ).ToString() ).Join( "/" )
		: _sharedGroupCounts.Sum( q => q.CountToShow( sourceSpaces ) ).ToString();

	public async Task<IEnumerable<SpaceToken>> GetSourceOptionsOn1Space( 
		SpaceState sourceSpaceState, 
		RemoveReason removeReason
	) {
		bool removing = removeReason != RemoveReason.None;

		IEnumerable<IToken> tokens = sourceSpaceState.OfAnyTag( RemainingTypes );

		// 'Removable' Filter on Tokens
		if(removing)
			tokens = await sourceSpaceState.WhereRemovable( tokens, removeReason );

		var spaceTokens = tokens.On( sourceSpaceState.Space );

		return spaceTokens;
	}


	#region configure

	public Quota AddGroup( int count, params ITokenClass[] classes ) {
		VerifyClasses( classes );
		_sharedGroupCounts.Add( new QuotaGroup( count, classes ) );
		return this;
	}

	public Quota AddAll( params ITokenClass[] classes ) {
		VerifyClasses( classes );
		_sharedGroupCounts.Add( new QuotaGroup( classes ) );
		return this;
	}

	static void VerifyClasses( ITokenClass[] classes ) {
		if(classes.Length == 0)
			throw new ArgumentOutOfRangeException( nameof( classes ), "Quota group musthave at least 1 class" );
	}


	public Quota BeVerbose() { _beVerbose=true; return this; }

	#endregion


	#region private

	ITokenClass[] CalcRemainingTypes() => _sharedGroupCounts
		.SelectMany( q => q.Classes )
		.Distinct()
		.ToArray();

	readonly List<QuotaGroup> _sharedGroupCounts = new(); // the # we push from each group
	ITokenClass[] _remainingTypes;
	bool _beVerbose = false;

	class QuotaGroup {

		/// <summary> Selects as many tokens of given classes as possible - no max. </summary>
		public QuotaGroup( params ITokenClass[] classes ) {
			_count = int.MaxValue;
			Classes = classes;
		}

		/// <summary> Selects up to specified # of tokens. </summary>
		public QuotaGroup( int count, params ITokenClass[] classes ) {
			_count = count;
			Classes = classes;
		}

		public int CountToShow( params SpaceState[] sources )
			=> Math.Min( _count, sources.Sum( s => s.SumAny( Classes ) ) );

		public string VerboseString( params SpaceState[] sources ) {
			return CountToShow( sources ) + " " + Classes.Select( c => c.Label ).Join( "/" );
		}

		public bool HasMore => 0 < _count;
		public bool Contains( ITokenClass @class ) => Classes.Contains( @class );
		public void UsedOne() { if(_count != int.MaxValue) --_count; }
		public ITokenClass[] Classes { get; private set; }

		public override string ToString() => $"{NumStr} {ClassLabelsStr}";
		string NumStr => _count == int.MaxValue ? "All" : _count.ToString();
		string ClassLabelsStr => Classes.Select( c => c.Label ).Join( "/" );

		int _count;

	}

	#endregion

}
