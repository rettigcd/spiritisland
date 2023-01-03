namespace SpiritIsland.JaggedEarth;

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit, GameState gameState, UnitOfWork uow ) 
		: base( spirit, gameState, uow ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}
