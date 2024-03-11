namespace SpiritIsland;

/// <param name="showSpaceInTextDescription">If all of the tokens are on the same space, don't show it in the text.</param>
public class SpaceToken : TokenLocation, IEquatable<SpaceToken> {

	#region constructor / deconstructor

	/// <summary>
	/// Captures the SpaceState so we can use it in the UI safely.
	/// </summary>
	/// <param name="spaceState"></param>
	/// <param name="token"></param>
#pragma warning disable IDE0290 // Use primary constructor
	public SpaceToken(SpaceState spaceState, IToken token) {
		SpaceState = spaceState;
		Token = token;
	}
#pragma warning restore IDE0290 // Use primary constructor

	public void Deconstruct(out Space space, out IToken token) {
		space = Space;
		token = Token;
	}

	#endregion constructor / deconstructor

	public Space Space => SpaceState.Space;
	public IToken Token { get; }
	ILocation TokenLocation.Location => SpaceState.Space;

	public SpaceState SpaceState { get; }

	#region IOption.Text config

	public string Text => ToString();

	#endregion IOption.Text config

	public Task<TokenMovedArgs> MoveTo( SpaceState destination, int count=1 )
		=> this.Token.MoveAsync(Space,destination.Space,count);

	public bool Exists => 0 < Count;
	public int Count => SpaceState[Token];
	public bool IsSacredSite => (Token is SpiritPresenceToken spt) && spt.Self.Presence.IsSacredSite(SpaceState);

	public Task Destroy() => SpaceState.Destroy( Token, 1 );
	public Task Remove() => SpaceState.RemoveAsync( Token, 1 );
	public Task<SpaceToken> Add1StrifeToAsync() {
		return SpaceState.Add1StrifeToAsync( Token.AsHuman() );
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
