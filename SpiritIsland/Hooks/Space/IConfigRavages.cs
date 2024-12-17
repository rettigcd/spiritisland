namespace SpiritIsland;

/// <remarks>async for (choosing if tokens sit-out ravage) and if we want to use the stop here.</remarks>
public interface IConfigRavages : ISpaceEntity {
	// Passing in Space because not all adjustments need RavageBehaviour, some adjust the Tokens exchange-timing.
	Task Config( Space space );
}
