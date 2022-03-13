namespace SpiritIsland;

public class TargetSpiritCtx : SelfCtx {

	public TargetSpiritCtx( Spirit self, GameState gs, Spirit target, Cause cause ) : base(self,gs,cause) {
		Other = target;
	}

	public Spirit Other { get; }

	public SelfCtx OtherCtx => new SelfCtx( Other, GameState, Cause );

}