namespace SpiritIsland;

public class NullBlightCard : IBlightCard {

	public bool CardFlipped {get; set; }

	public string Name => "[null]";
	public string Description => "[null]";
	string IOption.Text => Name;

	public IExecuteOn<GameCtx> Immediately => new DecisionOption<GameCtx>("no action", _ => { });

	public Task OnBlightDepleated( GameState gs ) {
		if(!CardFlipped) {
			CardFlipped = true;
			gs.blightOnCard += 4 * gs.Spirits.Length;
		}
		return Task.CompletedTask;
	}

	public void OnGameStart( GameState gs ) {
		gs.blightOnCard += 5 * gs.Spirits.Length;
	}

}