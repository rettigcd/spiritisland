namespace SpiritIsland;

/// <param name="prompt">Prompt to appear</param>
/// <param name="classString">Token classes to pre-select</param>
/// <exception cref="Exception"></exception>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class PreselectAttribute( string prompt, string classString, Present present = Present.Always ) : Attribute, IPreselect {

	readonly ITokenClass[] _tokenClasses = classString.Split( ',' )
		.Select( x => x switch {
			"Explorer" => (ITokenClass)Human.Explorer,
			"Town" => (ITokenClass)Human.Town,
			"City" => (ITokenClass)Human.City,
			"Beast" => (ITokenClass)Token.Beast,
			"Disease" => (ITokenClass)Token.Disease,
			"Wilds" => (ITokenClass)Token.Wilds,
			"Dahan" => (ITokenClass)Human.Dahan,
			_ => throw new Exception( $"{x} not known" )
		} ).ToArray();

	public async Task<Space?> PreSelect( Spirit spirit, Space[] spaces ) {
		var spaceTokenOptions = spaces
			.SelectMany( ss => ss.SpaceTokensOfAnyTag( _tokenClasses ) )
			.ToArray();

		SpaceToken? st = await spirit.SelectAsync( new A.SpaceTokenDecision( prompt, spaceTokenOptions, present ) );
		spirit.PreSelect(st);
		return st?.Space;
	}

}

/// <summary> Provides token/prompt info to enable selecing token and space at the same time (when appropriate). </summary>
public interface IPreselect {
	Task<Space?> PreSelect( Spirit spirit, Space[] spaces );
}

