namespace SpiritIsland;

/// <summary>
/// Only used for Powers.  So binding other spirits, uses starting spirits powers.
/// </summary>
public class TargetSpiritCtx : IHaveSpirit {

	public TargetSpiritCtx( Spirit self, Spirit target ) {
		Self = self;
		Other = target;
	}

	public Spirit Self { get; }
	public Spirit Other { get; }

	#region Parts from SelfCtx

	// ============  Parts from SelfCtx  =============

	public virtual void AddFear( int count ) => Self.AddFear(count);

	public Task<bool> YouHave( string elementString ) => Self.YouHave( elementString );

	public TargetSpaceCtx Target( Space space ) => Self.Target( space );

	#endregion

}