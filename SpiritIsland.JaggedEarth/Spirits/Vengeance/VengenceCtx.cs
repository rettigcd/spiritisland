namespace SpiritIsland.JaggedEarth;

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit, GameState gameState, UnitOfWork actionId, Cause cause ) : base( spirit, gameState, actionId, cause ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}
