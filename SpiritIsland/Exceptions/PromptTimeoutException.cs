namespace SpiritIsland;

/// <summary>
/// Thrown into the engine when a decision has been waiting on the user longer than
/// <see cref="UserGateway.PromptTimeout"/>. The stepwise SoloGame catches it, rolls back to its last
/// checkpoint and returns a Suspended result so the game can be saved and the process shut down.
/// </summary>
public class PromptTimeoutException() : Exception( "Timed out waiting for the user to make a decision." ) { }
