namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape : SpiritAction {

	static string CountToWord(int count) => count switch { int.MaxValue => "All", _ => count.ToString() };

	public PiecesEscape( int count = int.MaxValue ) 
		: base( $"{CountToWord(count)} pieces Escape" )
	{
		NumToEscape = count;
	}

	public override async Task ActAsync( SelfCtx ctx ) {

		int remaining = NumToEscape;
		var tokens = EndlessDark.Space.Tokens;
		while(0 < remaining) {
			// select token
			var options = tokens.Keys.Cast<IToken>().On( EndlessDark.Space ).ToList();
			SpaceToken? spaceToken = await ctx.SelectAsync(new A.SpaceToken( $"Select ({remaining}) piece(s) to escape Endless Darkness", options, Present.Always ) );
			if(spaceToken == null) return;
			--remaining;

			// select destination
			// !!! When selectin Destination for presence, need to check where they are allowed to be.  (no ocean inland, or lure on the coast!)
			var destination = await ctx.SelectAsync(new A.Space("Place escaped piece", ctx.Self.Presence.Spaces, Present.Always).ShowTokenLocation( spaceToken.Token )) 
				?? throw new GameOverException(new GameOver(GameOverResult.Defeat,"Unable to place escaped pieces."));
			await spaceToken.MoveTo(destination);
		}
	}
	public int NumToEscape { get; }
}
