namespace SpiritIsland;

public class TargetSpiritCtx : SelfCtx {

	public TargetSpiritCtx( SelfCtx ctx, Spirit target ) : base( ctx ) {
		Other = target;
	}

	public Spirit Other { get; }

	public SelfCtx OtherCtx => Other==Self 
		? this 
		: Other.BindMyPower( GameState, CurrentActionId ); // ??? !!! do we ever Target Other Spirits outside of powers.  I think not.

}