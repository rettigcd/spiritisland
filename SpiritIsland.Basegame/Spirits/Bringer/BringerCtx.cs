namespace SpiritIsland.Basegame;

public class BringerCtx : SelfCtx {
	public BringerCtx( Bringer bringer, GameState gs, UnitOfWork actionId ):base( bringer, gs, actionId, Cause.MyPowers ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);
}

