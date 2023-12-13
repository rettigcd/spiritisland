using SpiritIsland.A;

namespace SpiritIsland;

public class SpaceToken 
	: IOption
	, IEquatable<SpaceToken>
{
	#region constructor / deconstructor

	/// <param name="showSpaceInTextDescription">If all of the tokens are on the same space, don't show it in the text.</param>
	public SpaceToken( Space space, IToken token ) { 
		Space = space; 
		Token = token;
	}

	public void Deconstruct(out Space space, out IToken token) {
		space = Space;
		token = Token;
	}

	#endregion constructor / deconstructor

	public Space Space { get; }
	public IToken Token { get; }

	#region IOption.Text config

	public string Text => ToString();

	#endregion IOption.Text config

	public async Task<TokenMovedArgs> MoveTo( SpaceState destination, int count=1 ) {
		// Current implementation favors:
		//		switching token types prior to Add/Remove so events handlers don't switch token type
		//		perfoming the add/remove action After the Adding/Removing modifications

		// Possible problems to keep in mind:
		//		The token in the Added event, may be different than token that was attempted to be added.
		//		The Token in the Removed event, may be a different token than was requested to be removed.
		//		The token Added may be Different than the token Removed
		//		Move requires a special Publish because it pertains to 2 spaces - we don't want to publish it twice (once for each space)

		// Mitigating Factors
		//		The AddingToken args prevents changing the Count if it is a MoveTo

		var source = Space.Tokens;
		if(source[Token] < count) return null; // unable to (re)move desired token

		// Remove from source
		var (removeResult,removedNotifier) = await source.Remove_Silent( Token, count, RemoveReason.MovedFrom );
		if( removeResult.Count == 0 ) return null;

		// Add to destination
		var (addResult,addedNotifier) = await destination.Add_Silent( 
			removeResult.Removed, // possibly modified, NOT original
			removeResult.Count, // possibly modified
			AddReason.MovedTo
		);

		// Publish
		var tokenMoved = new TokenMovedArgs( removeResult, addResult );
		await removedNotifier.NotifyRemoved( tokenMoved );
		await addedNotifier.NotifyAdded( tokenMoved );

		return tokenMoved;
	}

	public bool Exists => 0 < Count;
	public int Count => Space.Tokens[Token];

	public Task Destroy() => Space.Tokens.Destroy( Token, 1 );
	public Task Remove() => Space.Tokens.RemoveAsync( Token, 1 );
	public Task<SpaceToken> Add1StrifeToAsync() {
		return Space.Tokens.Add1StrifeToAsync( Token.AsHuman() );
	}

	#region object overrides: GetHashCode/Equals/ToString

	public override int GetHashCode() => Space.GetHashCode()-Token.GetHashCode();

	public override bool Equals(object obj) => Equals(obj as SpaceToken);

	public bool Equals( SpaceToken other ) => other is not null  // Don't use .Equals() or == here
		&& other.Token == Token && other.Space == Space;

	public static bool operator ==(SpaceToken st1, SpaceToken st2) 
		=> st1 is null ? st2 is null : st1.Equals(st2);
    public static bool operator !=(SpaceToken st1, SpaceToken st2) => !(st1==st2);

	public override string ToString() => $"{Token.Text} on {Space.Label}";

	#endregion object overrides: GetHashCode/Equals/ToString
}