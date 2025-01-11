namespace SpiritIsland;

/// <summary>
/// Tracks how many tokens of a particular type are allowed to do something
/// </summary>
public class Quota : IQuota {

	/// <summary>
	/// Provides a description of the remaining tokens to be selected.
	/// </summary>
	public string RemainingTokenDescriptionOn(Space[] sourceSpaces) => _beVerbose
		? Remaining_GroupCounts(sourceSpaces)
		: Remaining_Sum(sourceSpaces);

	string Remaining_GroupCounts(Space[] sourceSpaces) => _quotaGroups.Select(x => x.CountAndTagOnSpaces(sourceSpaces)).Join("/"); // includes the Tags (token types)
	string Remaining_Sum(Space[] sourceSpaces) => _quotaGroups.Sum(q => q.CountOnSpaces(sourceSpaces)).ToString(); // just the sum

	public IEnumerable<SpaceToken> GetSourceOptionsOn1Space(Space sourceSpace) {
		return sourceSpace.OfAnyTag(RemainingTypes).On(sourceSpace);
	}

	public void MarkTokenUsed(ITokenLocation tl) {

		var match = _quotaGroups.First(group => group.Matches(tl.Token));

		match.UsedOne();
		if( !match.HasMore )
			_quotaGroups.Remove(match);

		_remainingTypes = null; // recalc next time neede
	}

	public Quota AddGroup(int count, params ITokenClass[] classes) {
		VerifyClasses(classes);
		if( 0 < count ) // Be safe, prevent 0 (command the beasts)
			_quotaGroups.Add(new QuotaGroup(count, classes));
		return this;
	}

	public Quota AddAll(params ITokenClass[] classes) {
		VerifyClasses(classes);
		_quotaGroups.Add(new QuotaGroup(classes));
		return this;
	}

	public Quota BeVerbose() { _beVerbose = true; return this; }


	#region private

	static void VerifyClasses(ITokenClass[] classes) {
		if( classes.Length == 0 )
			throw new ArgumentOutOfRangeException(nameof(classes), "Quota group musthave at least 1 class");
	}

	ITokenClass[] RemainingTypes => _remainingTypes ??= CalcRemainingTypes();

	ITokenClass[] CalcRemainingTypes() => _quotaGroups
		.SelectMany(q => q.Classes)
		.Distinct()
		.ToArray();

	readonly List<QuotaGroup> _quotaGroups = []; // the # we push from each group
	ITokenClass[]? _remainingTypes;
	bool _beVerbose = false;

	class QuotaGroup {

		/// <summary> Selects as many tokens of given classes as possible - no max. </summary>
		public QuotaGroup(params ITokenClass[] classes) {
			_count = int.MaxValue;
			Classes = classes;
		}

		/// <summary> Selects up to specified # of tokens. </summary>
		public QuotaGroup(int count, params ITokenClass[] classes) {
			_count = count;
			Classes = classes;
		}

		/// <summary> Limits count to tokens that are available. </summary>
		public int CountOnSpaces(params Space[] sources)
			=> Math.Min(_count, sources.Sum(s => s.SumAny(Classes)));

		/// <summary> Shows the # of available tokens in group. </summary>
		/// <example> 
		/// If we are gathering 4 Town/Explorer but there are only 2 tokens available, 
		/// shows "2 Town/Explorer"
		/// </example>
		public string CountAndTagOnSpaces(params Space[] sources) {
			return CountOnSpaces(sources) + " " + Classes.Select(c => c.Label).Join("/");
		}

		public bool HasMore => 0 < _count;
		public bool Matches(IToken token) => token.HasAny(Classes);
		public void UsedOne() { if( _count != int.MaxValue ) --_count; }
		public ITokenClass[] Classes { get; private set; }

		public override string ToString() => $"{NumStr} {ClassLabelsStr}";
		string NumStr => _count == int.MaxValue ? "All" : _count.ToString();
		string ClassLabelsStr => Classes.Select(c => c.Label).Join("/");

		int _count;

	}

	#endregion

}
