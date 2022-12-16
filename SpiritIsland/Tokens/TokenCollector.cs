namespace SpiritIsland;

/// <summary>
/// Collects/Moves tokens to a single space.
/// Doesn't specify the source of the spaces.
/// </summary>
public abstract class TokenCollector<DerivedType> where DerivedType : TokenCollector<DerivedType> {

	protected readonly TargetSpaceCtx _destinationCtx;
	readonly protected List<CollectQuota> sharedGroupCounts = new(); // the # we push from each group
	readonly protected Dictionary<TokenClass, int> indexLookupByClass = new(); // map from group back to its count

	protected TokenCollector(TargetSpaceCtx destinationCtx ) {
		_destinationCtx = destinationCtx;
	}

	protected abstract IEnumerable<SpaceState> PossibleGatherSources { get; }

	public DerivedType AddGroup( int countToGather, params TokenClass[] classes ) {
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
			var source = await _destinationCtx.Decision( Select.TokenFromManySpaces.ToCollect( prompt, _destinationCtx.Space, options, present ) );
			if(source == null) break;
			await _destinationCtx.Move( source.Token, source.Space, _destinationCtx.Space );
			MarkAsCollected( source );
			collected.Add( source );
		}
		return collected.ToArray();
	}

	protected TokenClass[] RemainingTypes => indexLookupByClass
		.Where( pair => sharedGroupCounts[pair.Value].count > 0 )
		.Select( pair => pair.Key )
		.ToArray();

	/// <returns>Specific tokens that can collected.</returns>
	/// <remarks>virtual because BeastGatherer can gather from extended range</remarks>
	protected virtual SpaceToken[] GetSpaceTokenOptions() => PossibleGatherSources
		.SelectMany( sourceSpaceState => sourceSpaceState
			.OfAnyType( RemainingTypes )
			.Select( tokens => new SpaceToken( sourceSpaceState.Space, tokens ) )
		)
		.ToArray();

	/// <summary> The remaining quota of each token class </summary>
	protected IEnumerable<CollectQuota> RemainingQuota => sharedGroupCounts.Where( g => g.count > 0 );
	protected void MarkAsCollected( SpaceToken source ) {
		--sharedGroupCounts[indexLookupByClass[source.Token.Class]].count;
	}

}

public class CollectQuota {

	public CollectQuota( int count, params TokenClass[] classes ) {
		this.count = count;
		this.classes = classes;
	}

	public int count;
	public TokenClass[] classes;
	public override string ToString() => count + " " + classes.Select( c => c.Label ).Join( "/" );
}
