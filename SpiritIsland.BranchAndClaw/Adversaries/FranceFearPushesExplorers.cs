namespace SpiritIsland.Basegame.Adversaries;

class FranceFearPushesExplorers : BaseModEntity, IModifyRemovingTokenAsync {

	// Fear Card effects never remove Explorer. If one would, you may instead Push that Explorer.
	public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		if(args.Token.Class == Human.Explorer 
			&& args.Reason == RemoveReason.Removed
			&& ActionScope.Current.Category == ActionCategory.Fear
			// Make sure Isolation tokens don't prevent the push,
			// If isolation tokens prevent explorer from moving, they may be removed
			&& args.From.IsConnected
			&& args.From.Adjacent.Any(x=>x.IsConnected)
		) {
			Spirit spirit = ActionScope.Current.Owner 
				?? args.From.Space.Boards.First().FindSpirit();

			await args.From.SourceSelector
				.AddGroup(args.Count,Human.Explorer)
				.Track( _ => --args.Count )
				.PushN(spirit);
		}
	}
}
