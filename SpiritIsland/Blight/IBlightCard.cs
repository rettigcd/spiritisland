namespace SpiritIsland;

public interface IBlightCard : IOption {
	string Name { get; }
	string Description { get; }
	int Side2BlightPerPlayer { get; }
	void OnGameStart( GameState gs );
	Task OnBlightDepleated( GameState gs );
	bool CardFlipped { get; set; } // set so we can update via Memento
	IActOn<GameState> Immediately { get; }
}
