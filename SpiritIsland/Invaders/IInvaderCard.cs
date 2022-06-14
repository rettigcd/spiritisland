
namespace SpiritIsland;

public interface IInvaderCard : IOption {

	int InvaderStage { get; }

	bool Skip { get; set; }
	bool HoldBack { get; set; }

	bool Matches( Space space );

	Task Build( GameState gameState );
	Task Explore( GameState gameState );
	Task Ravage( GameState gameState );
}

public interface SpaceFilter {
	bool Matches( Space space );
	string Text { get; }
}