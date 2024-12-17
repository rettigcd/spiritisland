namespace SpiritIsland;

/// <summary>
/// Runs at the beginning of EVERY Action
/// </summary>
public interface IRunAtStartOfEveryAction {
	Task Start(ActionScope current);
}

// !! This is only currently being used to queue up an action at the end of each action.
// !! maybe we should replace with IRunAtEndOfAction
// !! could also store in same bucket as IRunWhenTimePasses so there is only 1 bucket.
// !! OR - replace with IRunAtEndOfMySpiritsActions and store it in the Spirit.Mods

// Could scope to a single spirit: 
// interface IRunAtTheEndOfEachSpiritAction {}
// or
// interface IRunAtTheBeginningofEachSpiritAction {} // 