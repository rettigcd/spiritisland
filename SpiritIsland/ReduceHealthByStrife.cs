namespace SpiritIsland;

public class ReduceHealthByStrife
	: BaseModEntity
	, IModifyAddingToken
	, IModifyRemovingToken
	, IRunWhenTimePasses
{
	static public async Task Init( GameState gameState ){
		// adjust current strifed invaders
		foreach(var space in ActionScope.Current.Spaces)
			foreach(var invader in space.AllHumanTokens().Where(x=>0<x.StrifeCount).ToArray() )
				await space.AllHumans(invader).AdjustAsync(x=>x.AddHealth(-x.StrifeCount));

		var mod = new ReduceHealthByStrife();
		gameState.AddIslandMod(mod);
		gameState.AddTimePassesAction(mod); // cleanup
	}

	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ){
		if(args.Token is not HumanToken human || human.StrifeCount==0) return Task.CompletedTask;
		var adjustment = args.From.Humans(args.Count,human).Adjust(human=>human.AddHealth(human.StrifeCount));
		args.Token = adjustment.NewToken;
		return Task.CompletedTask;
	}

	Task IModifyAddingToken.ModifyAddingAsync( AddingTokenArgs args ){
		if(args.Token is not HumanToken human || human.StrifeCount==0 ) return Task.CompletedTask;
		var damaged = human.AddHealth(-human.StrifeCount);
		args.Token = damaged;
		return Task.CompletedTask;
	}

	#region IRunWhenTimePasses imp

	bool IRunWhenTimePasses.RemoveAfterRun => true;

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	Task IRunWhenTimePasses.TimePasses( GameState gameState ){
		foreach(var space in ActionScope.Current.Spaces)
			foreach(var invader in space.AllHumanTokens().Where(x=>0<x.StrifeCount).ToArray() )
				space.AllHumans(invader).Adjust(x=>x.AddHealth(x.StrifeCount));
		return Task.CompletedTask;
	}
	
	#endregion IRunWhenTimePasses imp

}
