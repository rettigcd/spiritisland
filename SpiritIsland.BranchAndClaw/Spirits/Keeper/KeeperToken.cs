namespace SpiritIsland.BranchAndClaw;

public class KeeperToken( Spirit _spirit ) : SpiritPresenceToken(_spirit), IHandleTokenAddedAsync {
	public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
		if(args.Added != this) return;

		int tokenCount = to[this];
		bool createdSacredSite = (tokenCount-args.Count) < 2 && 2<= tokenCount;

		if(createdSacredSite && to.Dahan.Any)
			await Self.Target( (Space)args.To ).PushDahan( int.MaxValue );

	}
}