namespace SpiritIsland;

public interface IActionFactory : IOption {

	Task ActivateAsync( Spirit self );

	bool CouldActivateDuring( Phase speed, Spirit spirit );

	/// <summary> Should be alias for IOption.Text </summary>
	string Title { get; }

}

public interface IPowerActionFactory : IActionFactory {
	Phase Speed { get; }
	string RangeText { get; }
	string TargetFilter { get; }
}
