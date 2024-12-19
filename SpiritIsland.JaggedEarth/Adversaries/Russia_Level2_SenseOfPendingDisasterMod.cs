namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Pushes an explorer instead of destroying it.
/// </summary>
class Russia_Level2_SenseOfPendingDisasterMod : BaseModEntity, IModifyRemovingToken {

	async Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		const string key = "A Sense of Pending Disaster";
		Space[] pushOptions;
		var scope = ActionScope.Current;
		if(args.Token.Class == Human.Explorer     // Is explorer
			&& args.Reason == RemoveReason.Destroyed // destroying
			&& !ActionScope.Current.ContainsKey( key )  // first time
			&& 0 < (pushOptions = args.From.Adjacent_ForInvaders.IsInPlay().ToArray()).Length
		) {
			--args.Count; // destroy one fewer
			scope[key] = true; // don't save any more

			Spirit spirit = scope.Owner ?? args.From.SpaceSpec.Boards[0].FindSpirit();
			Space destination = await spirit.SelectAlways( A.SpaceDecision.ToPushToken( args.Token, args.From, pushOptions, Present.Always ) );
			await args.Token.MoveAsync( args.From, destination );
		}
	}

}