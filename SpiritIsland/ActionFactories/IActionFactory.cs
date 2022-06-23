namespace SpiritIsland;

public class ActionTaken {
	public IActionFactory ActionFactory { get; }
	public object Context { get; }
	public ActionTaken(IActionFactory actionFactory, object context ) {
		this.ActionFactory = actionFactory;
		this.Context = context;
	}
}

public interface IActionFactory : IOption {

	Task ActivateAsync( SelfCtx ctx ); // returns Target if any

	bool CouldActivateDuring( Phase speed, Spirit spirit );

	/// <summary> Should be alias for IOption.Text </summary>
	string Name { get; }

}

public interface IFlexibleSpeedActionFactory : IActionFactory {
	Phase DisplaySpeed { get; }
	/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
	ISpeedBehavior OverrideSpeedBehavior { get; set; }
}

public interface IRecordLastTarget {
	public object LastTarget { get; }
}
