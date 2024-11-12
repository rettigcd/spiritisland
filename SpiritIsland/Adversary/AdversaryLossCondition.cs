namespace SpiritIsland;

public class AdversaryLossCondition( string description, Action<GameState> additionalWinLossCondition ) {
	public virtual void Init( GameState gs ) {
		gs.AddWinLossCheck( _additionalWinLossCondition );
	}
	public string Description { get; } = description;
	readonly protected Action<GameState> _additionalWinLossCondition = additionalWinLossCondition;
}