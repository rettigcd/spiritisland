using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Does not change the logical token in the space, but makes Property-adjustments to it.
/// </summary>
/// <remarks>
/// Since the Tokens are immutable, instead of changing their properties, replaces with a new Token with desired properties.
/// </remarks>
public class HumanAdjustBinding {

	#region constructors

	/// <summary> Replaces All tokens of given type</summary>
	public HumanAdjustBinding( Space ss, HumanToken tokenToReplace ) : this( ss, ss[tokenToReplace], tokenToReplace ) { }

	/// <summary> Replaces N tokens of given type</summary>
	public HumanAdjustBinding( Space ss, int countToReplace, HumanToken tokenToReplace ) {
		ArgumentOutOfRangeException.ThrowIfNegative( countToReplace );
		_ss = ss;
		_count = countToReplace;
		_token = tokenToReplace;
	}

	#endregion constructors

	#region public High-Level changes

	/// <summary> Replaces (via adjust) HealthToken with new HealthTokens </summary>
	/// <returns> The # of remaining Adjusted tokens. </returns>
	public async Task<HumanAdjustment> AdjustHealthAsync( int delta ) {
		return await AdjustAsync( x=>x.AddHealth(delta) );
	}

	public HumanAdjustment Heal() => Adjust( x=>x.Healthy );

	#endregion public High-Level changes


	public HumanAdjustment Adjust( Func<HumanToken,HumanToken> adjustment ) {
		if(_count == 0) return NoChange;
		HumanToken newToken = adjustment( _token );
		if(newToken == _token) return NoChange;

		// Make change
		_ss.Adjust( _token, -_count );
		_ss.Adjust( newToken, _count );
		var result = new HumanAdjustment( _count, _token, newToken );
		ActionScope.Current.Log( result );

		_token = newToken; // do AFTER the event is constructed.
		return result;
	}

	/// <summary> Use when the new replaced token might be destroyed. </summary>
	public async Task<HumanAdjustment> AdjustAsync( Func<HumanToken, HumanToken> adjustment ) {
		if(_count==0) return NoChange;
		HumanToken newToken = adjustment( _token );
		if( newToken == _token ) return NoChange;

		_ss.Adjust( _token, -_count );
		await _ss.AdjustUpOrDestroyAsync( newToken, _count );
		var result = new HumanAdjustment( _count, _token, newToken );
		ActionScope.Current.Log( result );

		_token = newToken; // do AFTER the event is constructed.
		return result;
	}

	#region private
	HumanAdjustment NoChange => new HumanAdjustment(0,_token,_token);
	readonly Space _ss;
	readonly int _count;
	HumanToken _token;
	#endregion private
}

public record HumanAdjustment( int Count, HumanToken OldToken, HumanToken NewToken ) : ILogEntry {
	public LogLevel Level => LogLevel.Debug;

	public string Msg( LogLevel level ) => $"Replace {Count} {OldToken.SpaceAbreviation} with {NewToken.SpaceAbreviation}";
}
