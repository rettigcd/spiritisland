namespace SpiritIsland.BranchAndClaw;

public class KeeperToken : SpiritPresenceToken, IHandleTokenAddedAsync {

	public KeeperToken(Spirit spirit):base(spirit) {}

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != this) return;

		int tokenCount = args.To[this];
		bool createdSacredSite = (tokenCount-args.Count) < 2 && 2<= tokenCount;

		if(createdSacredSite && args.To.Dahan.Any)
			await Self.Target( args.To.Space ).PushDahan( int.MaxValue );

	}
}