namespace SpiritIsland;

/// <param name="showSpaceInTextDescription">If all of the tokens are on the same space, don't show it in the text.</param>
public class SpaceToken : ITokenLocation, IEquatable<SpaceToken> {

	#region constructor / deconstructor

	/// <summary>
	/// Captures the Space so we can use it in the UI safely.
	/// </summary>
	/// <param name="space"></param>
	/// <param name="token"></param>
#pragma warning disable IDE0290 // Use primary constructor
	public SpaceToken(Space space, IToken token) {
		Space = space;
		Token = token;
	}
#pragma warning restore IDE0290 // Use primary constructor

	#endregion constructor / deconstructor

	public Space Space { get; }
	public IToken Token { get; }
	ILocation ITokenLocation.Location => Space;

	#region IOption.Text config

	string IOption.Text => ToString();

	#endregion IOption.Text config

	public Task<TokenMovedArgs?> MoveTo( ILocation destination, int count=1 )
		=> new Move(this,destination).Apply(count);

	public bool Exists => 0 < Count;
	public int Count => Space[Token];
	public bool IsSacredSite => (Token is SpiritPresenceToken spt) && spt.Self.Presence.IsSacredSite(Space);

	public Task Destroy() => Space.Destroy( Token, 1 );
	public Task Remove() => Space.RemoveAsync( Token, 1 );
	public Task<SpaceToken> Add1StrifeToAsync() {
		return Space.Add1StrifeToAsync( Token.AsHuman() );
	}

	#region object overrides: GetHashCode/Equals/ToString

	public override int GetHashCode() => Space.SpaceSpec.GetHashCode()-Token.GetHashCode();

	public override bool Equals(object? obj) => Equals(obj as SpaceToken);

	public bool Equals( SpaceToken? other ) => other is not null  // Don't use .Equals() or == here
		&& other.Token == Token && other.Space == Space;

	public static bool operator ==(SpaceToken st1, SpaceToken? st2) 
		=> st1 is null ? st2 is null : st1.Equals(st2);
    public static bool operator !=(SpaceToken st1, SpaceToken st2) => !(st1==st2);

	public override string ToString() => $"{Token.Text} on {Space.SpaceSpec.Label}";

	#endregion object overrides: GetHashCode/Equals/ToString
}
