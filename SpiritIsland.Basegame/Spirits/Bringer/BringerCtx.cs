namespace SpiritIsland.Basegame;

public class BringerCtx : SelfCtx {
	public BringerCtx( Spirit spirit, GameState gs, UnitOfWork actionId ):base( spirit, gs, actionId ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);
}

