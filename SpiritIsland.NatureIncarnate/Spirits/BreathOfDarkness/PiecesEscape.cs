namespace SpiritIsland.NatureIncarnate;

public class PiecesEscape : GrowthActionFactory {

	public override string Name => "PiecesEscape" + (NumToEscape switch{ int.MaxValue => "", _ => $"({NumToEscape})" });

	public PiecesEscape(int count = int.MaxValue):base() {
		NumToEscape = count;
	}
	public override async Task ActivateAsync( SelfCtx ctx ) {

		int remaining = NumToEscape;
		var tokens = EndlessDark.Space.Tokens;
		while(0 < remaining) {
			// select token
			var options = tokens.Keys.Cast<IToken>().ToList();
			SpaceToken? spaceToken = await ctx.Decision(new Select.ASpaceToken( $"Select ({remaining}) piece(s) to escape Endless Darkness", EndlessDark.Space, options, Present.Always ) );
			if(spaceToken == null) return;
			--remaining;

			// select destination
			// !!! When selectin Destination for presence, need to check where they are allowed to be.  (no ocean inland, or lure on the coast!)
			var destination = await ctx.Decision(new Select.ASpace("Place escaped piece", ctx.Self.Presence.Spaces.Tokens(), Present.Always, spaceToken.Token)) 
				?? throw new GameOverException(new GameOver(GameOverResult.Defeat,"Unable to place escaped pieces."));
			await spaceToken.MoveTo(destination);
		}
	}
	public int NumToEscape { get; }
}
