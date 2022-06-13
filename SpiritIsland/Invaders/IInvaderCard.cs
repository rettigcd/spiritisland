
namespace SpiritIsland;

public interface IInvaderCard : IOption {
	// string Text { get; } - use IOption.Text
	int InvaderStage { get; }
	bool Escalation { get; }

	bool Skip { get; set; }
	bool HoldBack { get; set; }

	bool Matches( Space space );

	Task Build( GameState gameState );
	Task Explore( GameState gameState );
	Task Ravage( GameState gameState );
}