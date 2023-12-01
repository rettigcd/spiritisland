namespace SpiritIsland;

/// <summary>
/// Only used for Powers.  So binding other spirits, uses starting spirits powers.
/// </summary>
public class TargetSpiritCtx : SelfCtx {

	public TargetSpiritCtx( SelfCtx ctx, Spirit target ) : base( ctx.Self ) {
		Other = target;
	}

	public Spirit Other { get; }

	public SelfCtx OtherCtx => Other==Self ? this : new SelfCtx( Other );
}