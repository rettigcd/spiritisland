namespace SpiritIsland;

/// <summary>
/// Collects/Moves tokens to a single space.
/// Doesn't specify the source of the spaces.
/// </summary>
public abstract class TokenCollector<DerivedType> where DerivedType : TokenCollector<DerivedType> {


	protected readonly SpaceState _destinationTokens;
	protected readonly Spirit _self;

	readonly protected List<CollectQuota> sharedGroupCounts = new(); // the # we push from each group
	readonly protected Dictionary<IEntityClass, int> indexLookupByClass = new(); // map from group back to its count

	protected TokenCollector(Spirit self, SpaceState destinatinTokens ) {
		_self = self;
		_destinationTokens = destinatinTokens;
	}

	protected abstract IEnumerable<SpaceState> PossibleGatherSources { get; }

	public DerivedType AddGroup( int countToGather, params IEntityClass[] classes ) {
		int countIndex = sharedGroupCounts.Count;
		sharedGroupCounts.Add( new CollectQuota( countToGather, classes ) );
		foreach(var tokenClass in classes)
			indexLookupByClass.Add( tokenClass, countIndex );
		return (DerivedType)this;
	}

	/// <summary> Move-To or Gather </summary>
	async protected Task<SpaceToken[]> Collect( string actionPromptPrefix, Present present ) {

		var collected = new List<SpaceToken>();

		SpaceToken[] options;
		while(0 < (options = GetSpaceTokenOptions()).Length) {
			// !! maybe make the next line virtual instead of the GroupsToGather
			string prompt = actionPromptPrefix + RemainingQuota.Select( x => x.ToString() ).Join( ", " );
			var source = await _self.Gateway.Decision( Select.TokenFromManySpaces.ToCollect( prompt, options, present, _destinationTokens.Space ) );
			if(source == null) break;
			await source.Token.Move( source.Space.Tokens, _destinationTokens );
			MarkAsCollected( source );
			collected.Add( source );
		}
		return collected.ToArray();
	}

	protected IEntityClass[] RemainingTypes => indexLookupByClass
		.Where( pair => sharedGroupCounts[pair.Value].count > 0 )
		.Select( pair => pair.Key )
		.ToArray();

	/// <returns>Specific tokens that can collected.</returns>
	/// <remarks>virtual because BeastGatherer can gather from extended range</remarks>
	protected virtual SpaceToken[] GetSpaceTokenOptions() => PossibleGatherSources
		.SelectMany( sourceSpaceState => sourceSpaceState
			.RemovableOfAnyClass( RemoveReason.MovedFrom, RemainingTypes )
			.Select( tokens => new SpaceToken( sourceSpaceState.Space, (IToken)tokens ) )
		)
		.ToArray();

	/// <summary> The remaining quota of each token class </summary>
	protected IEnumerable<CollectQuota> RemainingQuota => sharedGroupCounts.Where( g => g.count > 0 );
	protected void MarkAsCollected( SpaceToken source ) {
		--sharedGroupCounts[indexLookupByClass[source.Token.Class]].count;
	}

}

public class CollectQuota {

	public CollectQuota( int count, params IEntityClass[] classes ) {
		this.count = count;
		this.classes = classes;
	}

	public int count;
	public IEntityClass[] classes;
	public override string ToString() => count + " " + classes.Select( c => c.Label ).Join( "/" );
}
