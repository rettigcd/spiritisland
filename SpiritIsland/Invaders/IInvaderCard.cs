
namespace SpiritIsland;

public interface IInvaderCard : IOption {

	bool Flipped { get; set; } // set is only public for Rewind
	void Flip();

	int InvaderStage { get; }

	bool Skip { get; set; }
	bool HoldBack { get; set; }

	bool MatchesCard( SpaceState space );

	bool HasEscalation { get; set; }

	Task Ravage( GameState gameState );
}
