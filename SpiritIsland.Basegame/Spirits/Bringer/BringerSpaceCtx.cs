namespace SpiritIsland.Basegame;

public class BringerSpaceCtx : TargetSpaceCtx {

	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	protected override InvaderBinding GetInvaders() => new ToDreamAThousandDeaths( this );
	
	public override DahanGroupBinding Dahan	=>  new DreamingDahan( Tokens.Dahan, ActionCtx );
}

public class DreamingDahan : DahanGroupBinding {
	public DreamingDahan( DahanGroupBindingNoEvents src, UnitOfWork uow ) : base( src, uow ) { }
	public override Task<int> DestroyToken( int _, HealthToken _1 ) => Task.FromResult(0);
}