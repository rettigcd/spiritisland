namespace SpiritIsland;

public class TargetSpiritCtx : SelfCtx {

	public TargetSpiritCtx( SelfCtx ctx, Spirit target ) : base( ctx ) {
		Other = target;
	}

	public Spirit Other { get; }

	public SelfCtx OtherCtx => Other==Self 
		? this 
		: Self.BindMyPowers( Other,GameState,ActionCtx ); // ??? !!! do we ever Target Other Spirits outside of powers.  I think not.
		// Vengeance has special rules when using its powers, need to be able to bind those powers to other spirits.

}