namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class PreselectAttribute : Attribute, IPreselect {

	/// <param name="prompt">Prompt to appear</param>
	/// <param name="classString">Token classes to pre-select</param>
	/// <exception cref="Exception"></exception>
	public PreselectAttribute( string prompt, string classString, Present present = Present.Always ) {
		_prompt = prompt;
		_tokenClasses = classString.Split( ',' )
			.Select( x => x switch {
				"Explorer" => (IEntityClass)Human.Explorer,
				"Town" => (IEntityClass)Human.Town,
				"City" => (IEntityClass)Human.City,
				"Beast" => (IEntityClass)Token.Beast,
				"Disease" => (IEntityClass)Token.Disease,
				"Wilds" => (IEntityClass)Token.Wilds,
				"Dahan" => (IEntityClass)Human.Dahan,
				_ => throw new Exception( $"{x} not known" )
			} ).ToArray();
		_present = present;
	}

	readonly string _prompt;
	readonly IEntityClass[] _tokenClasses;
	readonly Present _present;

	public async Task<Space> PreSelect( Spirit spirit, SpaceState[] spaces ) {
		var spaceTokenOptions = spaces
			.SelectMany( ss => ss.SpaceTokensOfAnyClass( _tokenClasses ) )
			.ToArray();

		SpaceToken st = await spirit.Gateway.Decision( new Select.ASpaceToken( _prompt, spaceTokenOptions, _present ) );
		spirit.Gateway.Preloaded = st;
		return st?.Space;
	}

}

/// <summary> Provides token/prompt info to enable selecing token and space at the same time (when appropriate). </summary>
public interface IPreselect {
	Task<Space> PreSelect( Spirit spirit, SpaceState[] spaces );
}

