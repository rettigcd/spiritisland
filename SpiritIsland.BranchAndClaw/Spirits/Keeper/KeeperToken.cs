namespace SpiritIsland.BranchAndClaw;

public class KeeperToken : SpiritPresenceToken, IHandleTokenAddedAsync {

	public KeeperToken(Spirit spirit):base(spirit) {}

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != this) return;

		var toTokens = args.To.Tokens;
		int tokenCount = toTokens[this];
		bool createdSacredSite = (tokenCount-args.Count) < 2 && 2<= tokenCount;

		if(createdSacredSite && toTokens.Dahan.Any)
			await Self.Target( args.To ).PushDahan( int.MaxValue );

	}
}