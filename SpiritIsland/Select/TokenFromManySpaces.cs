namespace SpiritIsland.Select;

// For Selecting Token from multiple spaces
public class TokenFromManySpaces : TypedDecision<SpaceToken>, IHaveAdjacentInfo {

	static public TokenFromManySpaces ToGather(
		string prompt,
		SpiritIsland.Space to,
		IEnumerable<SpaceToken> tokens,
		Present present
	) => new TokenFromManySpaces(
		prompt,
		tokens,
		present
	) {
		AdjacentInfo = new AdjacentInfo {
			Original = to,
			Adjacent = tokens.Select( s => s.Space ).Distinct().ToArray(),
			Direction = AdjacentDirection.Incoming
		}
	};

	public TokenFromManySpaces(
		string prompt,
		IEnumerable<SpaceToken>tokens,
		Present present
	)
		: base( prompt, tokens, present ) 
	{}

	public AdjacentInfo AdjacentInfo { get; set; }

}