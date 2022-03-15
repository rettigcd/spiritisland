
namespace SpiritIsland;

public interface IInvaderCard : IOption {
	string Text { get; }
	int InvaderStage { get; }
	bool Escalation { get; }

	bool Matches( Space space );

	Task Build( GameState gameState );
	Task Explore( GameState gameState );
	Task Ravage( GameState gameState );
}