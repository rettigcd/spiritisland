namespace SpiritIsland.Basegame;

public class BringerSpaceCtx : TargetSpaceCtx {

	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	protected override InvaderBinding GetInvaders() => new ToDreamAThousandDeaths( this );
	
	public override DahanGroupBinding Dahan	=>  new DreamingDahan( Tokens.Dahan, CurrentActionId );
}

public class DreamingDahan : DahanGroupBinding {
	public DreamingDahan( DahanGroupBindingNoEvents src, UnitOfWork uow ) : base( src, uow ) { }
	public override Task<PublishTokenRemovedArgs> Destroy( int _, HealthToken _1 ) => Task.FromResult((PublishTokenRemovedArgs)null);
}