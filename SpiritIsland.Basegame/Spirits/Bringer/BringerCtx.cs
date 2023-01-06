namespace SpiritIsland.Basegame;

public class BringerCtx : SelfCtx {
	public BringerCtx( Spirit spirit, GameState gs, UnitOfWork actionScope ):base( spirit, gs, actionScope ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);
}

