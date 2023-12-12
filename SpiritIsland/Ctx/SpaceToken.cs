using SpiritIsland.A;

namespace SpiritIsland;

public class SpaceToken : IOption {

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

	public string Text => ConfigText switch {
		ConfigSpaceTokenText.Token => Token.Text,
		ConfigSpaceTokenText.Space => Space.Label,
		_ => ToString(),
	};

	/// <summary> Set just before a Decision is made so text-interface only verbose when it needs to be. </summary>
	public ConfigSpaceTokenText ConfigText { get; set; }

	public enum ConfigSpaceTokenText { Token, Space, Both };

	#endregion IOption.Text config

	public Task<TokenMovedArgs> MoveTo(SpaceState destination) 
		=> Space.Tokens.MoveTo( Token, destination );

	public bool Exists => 0 < Count;
	public int Count => Space.Tokens[Token];

	public Task Destroy() => Space.Tokens.Destroy( Token, 1 );
	public Task Remove() => Space.Tokens.RemoveAsync( Token, 1 );
	public Task<SpaceToken> Add1StrifeToAsync() {
		return Space.Tokens.Add1StrifeToAsync( Token.AsHuman() );
	}

	#region object overrides: GetHashCode/Equals/ToString
	public override int GetHashCode() => Space.GetHashCode()-Token.GetHashCode();
	public override bool Equals(object obj) => obj is SpaceToken st && st.Token == Token && st.Space == Space;
	public override string ToString() => $"{Token.Text} on {Space.Label}";
	#endregion object overrides: GetHashCode/Equals/ToString
}