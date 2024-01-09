using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Does not change the logical token in the space, but makes Property-adjustments to it.
/// </summary>
/// <remarks>
/// Since the Tokens are immutable, instead of changing their properties, replaces with a new Token with desired properties.
/// </remarks>
public class PropertyAdjustment {
	readonly SpaceState _ss;
	readonly int _countToReplace;
	readonly IToken _tokenToReplace;
	/// <summary> Replaces All tokens of given type</summary>
	public PropertyAdjustment( SpaceState ss, IToken tokenToReplace ) : this( ss, ss[tokenToReplace], tokenToReplace ) { }
	/// <summary> Replaces N tokens of given type</summary>
	public PropertyAdjustment( SpaceState ss, int countToReplace, IToken tokenToReplace ) {
		if(countToReplace<0) throw new ArgumentOutOfRangeException(nameof(countToReplace));
		_ss = ss;
		_countToReplace = countToReplace;
		_tokenToReplace = tokenToReplace;
	}
	public TokensPropAdjustment To( HumanTokenClass newTokenType ) => To( _ss.GetDefault( newTokenType ) );
	public TokensPropAdjustment To( IToken newToken ) {
		_ss.Adjust( _tokenToReplace, -_countToReplace );
		_ss.Adjust( newToken, _countToReplace );
		var result = new TokensPropAdjustment( _countToReplace, _tokenToReplace, newToken );
		ActionScope.Current.Log( result );
		return result;
	}
	/// <summary> Use when the new replaced token might be destroyed. </summary>
	public async Task<TokensPropAdjustment> WithHumanAsync( HumanToken possiblyDestroyedNewToken ) {
		_ss.Adjust( _tokenToReplace, -_countToReplace );
		await _ss.AdjustUpHumanAsync( possiblyDestroyedNewToken, _countToReplace );
		var result = new TokensPropAdjustment( _countToReplace, _tokenToReplace, possiblyDestroyedNewToken );
		ActionScope.Current.Log( result );
		return result;
	}
}

public record TokensPropAdjustment( int Count, IToken OldToken, IToken NewToken ) : ILogEntry {
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel level ) => $"Replace {Count} {OldToken.Text} with {NewToken.Text}";
}
