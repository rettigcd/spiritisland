namespace SpiritIsland.Select;

// For Selecting Token from multiple spaces
public class TokenFromManySpaces : TypedDecision<SpaceToken>, IHaveAdjacentInfo {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public TokenFromManySpaces ToCollect(
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
			Central = to,
			Adjacent = Array.Empty<SpiritIsland.Space>(), // don't draw generic space-arrows, let the TokenSpace drawer, draw the better arrows   //  tokens.Select( s => s.Space ).Distinct().ToArray(),
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

	public AdjacentInfo AdjacentInfo { get; set; } // Incoming - when collecting (gathering / moving)

}