namespace SpiritIsland.Basegame;

public class BringerSpaceCtx : TargetSpaceCtx {

	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	protected override InvaderBinding GetInvaders() => new ToDreamAThousandDeaths( this );
	
	public override DahanGroupBinding Dahan	=>  new DreamingDahan( Tokens.Dahan, CurrentActionId );
}

public class DreamingDahan : DahanGroupBinding {
	public DreamingDahan( DahanGroupBindingNoEvents src, UnitOfWork uow ) : base( src, uow ) { }

	// Called from .Move() and .Dissolve the Bonds
	public override async Task<Token> Remove1( RemoveReason reason, Token toRemove = null ) {
		return reason == RemoveReason.Destroyed ? null
			: await base.Remove1( reason, toRemove );
	}

}