namespace SpiritIsland;

/// <summary>
/// Only used for Powers.  So binding other spirits, uses starting spirits powers.
/// </summary>
public class TargetSpiritCtx : SelfCtx {

	public TargetSpiritCtx( SelfCtx ctx, Spirit target ) : base( ctx ) {
		Other = target;
	}

	public Spirit Other { get; }

	public SelfCtx OtherCtx => Other==Self 
		? this 
		: Self.BindMyPowers( Other ); // This is ONLY used for Powers
		// Vengeance has special rules when using its powers, need to be able to bind those powers to other spirits.

}