namespace SpiritIsland;

/// <summary>
/// Tracks how many tokens of a particular type are allowed to do something
/// </summary>
public class Quota {

	public void MarkTokenUsed( IToken token ) {

		var match = _sharedGroupCounts.First( group => group.Matches( token ) );

		match.UsedOne();
		if(!match.HasMore)
			_sharedGroupCounts.Remove( match );

		_remainingTypes = null; // recalc next time neede
	}

	public ITokenClass[] RemainingTypes => _remainingTypes ??= CalcRemainingTypes();

	public string RemainingTokenDescriptionOn( Space[] sourceSpaces ) => _beVerbose 
		? _sharedGroupCounts.Select( x => x.VerboseString( sourceSpaces ).ToString() ).Join( "/" )
		: _sharedGroupCounts.Sum( q => q.CountToShow( sourceSpaces ) ).ToString();

	public IEnumerable<SpaceToken> GetSourceOptionsOn1Space( 
		Space sourceSpace
	) {
		return sourceSpace.OfAnyTag( RemainingTypes )
			.OnScopeTokens1( sourceSpace.SpaceSpec );
	}


	#region configure

	public Quota AddGroup( int count, params ITokenClass[] classes ) {
		VerifyClasses( classes );
		if(0<count) // Be safe, prevent 0 (command the beasts)
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

	readonly List<QuotaGroup> _sharedGroupCounts = []; // the # we push from each group
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

		public int CountToShow( params Space[] sources )
			=> Math.Min( _count, sources.Sum( s => s.SumAny( Classes ) ) );

		public string VerboseString( params Space[] sources ) {
			return CountToShow( sources ) + " " + Classes.Select( c => c.Label ).Join( "/" );
		}

		public bool HasMore => 0 < _count;
		public bool Matches( IToken token ) => token.HasAny( Classes );
		public void UsedOne() { if(_count != int.MaxValue) --_count; }
		public ITokenClass[] Classes { get; private set; }

		public override string ToString() => $"{NumStr} {ClassLabelsStr}";
		string NumStr => _count == int.MaxValue ? "All" : _count.ToString();
		string ClassLabelsStr => Classes.Select( c => c.Label ).Join( "/" );

		int _count;

	}

	#endregion

}
