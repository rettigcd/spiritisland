namespace SpiritIsland;

/// <summary>
/// Tracks how many tokens of a particular type are allowed to do something
/// </summary>
public class Quota {

	public void MarkTokenUsed( IToken token ) {
		RemainingQuota.First( q => q.Contains( token.Class ) ).UsedOne();
		_remainingTypes = null; // recalc next time neede
	}

	public IEntityClass[] RemainingTypes => _remainingTypes ??= CalcRemainingTypes();

	public string RemainingTokenDescriptionOn( SpaceState[] sourceSpaces )
		=> RemainingQuota.Sum( q => q.CountToShow( sourceSpaces ) ).ToString();
	//  => RemainingQuota.Select( x => x.VerboseString( sourceSpaces ).ToString() ).Join( ", " );

	public Quota AddGroup( int count, params IEntityClass[] classes ) {
		_sharedGroupCounts.Add( new QuotaGroup( count, classes ) );
		return this;
	}

	public Quota AddAll( params IEntityClass[] classes ) {
		_sharedGroupCounts.Add( new QuotaGroup( classes ) );
		return this;
	}

	IEntityClass[] CalcRemainingTypes() => RemainingQuota
		.SelectMany( q => q.Classes )
		.Distinct()
		.ToArray();

	IEnumerable<QuotaGroup> RemainingQuota => _sharedGroupCounts.Where( g => g.HasMore );

	readonly List<QuotaGroup> _sharedGroupCounts = new(); // the # we push from each group
	IEntityClass[] _remainingTypes;

	class QuotaGroup {

		/// <summary> Selects as many tokens of given classes as possible - no max. </summary>
		public QuotaGroup( params IEntityClass[] classes ) {
			_count = int.MaxValue;
			Classes = classes;
		}

		/// <summary> Selects up to specified # of tokens. </summary>
		public QuotaGroup( int count, params IEntityClass[] classes ) {
			_count = count;
			Classes = classes;
		}

		public int CountToShow( params SpaceState[] sources )
			=> Math.Min( _count, sources.Sum( s => s.SumAny( Classes ) ) );

		public string VerboseString( params SpaceState[] sources ) {
			return CountToShow( sources ) + " " + Classes.Select( c => c.Label ).Join( "/" );
		}

		public bool HasMore => 0 < _count;
		public bool Contains( IEntityClass @class ) => Classes.Contains( @class );
		public void UsedOne() { if(_count != int.MaxValue) --_count; }
		public IEntityClass[] Classes { get; private set; }

		public override string ToString() => $"{NumStr} {ClassLabelsStr}";
		string NumStr => _count == int.MaxValue ? "All" : _count.ToString();
		string ClassLabelsStr => Classes.Select( c => c.Label ).Join( "/" );

		int _count;

	}

}
