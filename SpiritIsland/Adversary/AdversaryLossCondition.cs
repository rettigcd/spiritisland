#nullable enable
namespace SpiritIsland;

public class AdversaryLossCondition( string description, Action<GameState>? additionalWinLossCondition = null ) {
	public virtual void Init( GameState gs ) {
		if( _additionalWinLossCondition is not null )
			gs.AddWinLossCheck( _additionalWinLossCondition );
	}
	public string Description { get; } = description;
	readonly protected Action<GameState>? _additionalWinLossCondition = additionalWinLossCondition;
}