namespace SpiritIsland;

public interface IExecuteOn<CTX> {

	/// <summary>
	/// Dual Purpose: 
	/// 1) detects if action can be performed on context
	/// 2) detects if pre-requisite condition has been triggered
	/// </summary>
	bool IsApplicable(CTX ctx);

	/// <summary>
	/// Describes the action that will be taken. (ie. Push 2 Beast)
	/// </summary>
	/// <remarks>This is not the Title of a card unless the Title provides a description of the action.</remarks>
	string Description { get; }

	Task Execute(CTX ctx);
}
