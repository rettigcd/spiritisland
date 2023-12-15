namespace SpiritIsland;

public class SpaceToken 
	: TokenOn
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
	ILocation TokenOn.Source => Space;

	#region IOption.Text config

	public string Text => ToString();

	#endregion IOption.Text config

	public Task<TokenMovedArgs> MoveTo( SpaceState destination, int count=1 )
		=> this.Token.MoveAsync(Space,destination.Space,count);

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
