namespace SpiritIsland.Agent;

/// <summary> What an agent may choose when starting a game. </summary>
public record NewGameRequest( string Spirit, string Board, int Seed, string? Adversary = null, int AdversaryLevel = 0 );
