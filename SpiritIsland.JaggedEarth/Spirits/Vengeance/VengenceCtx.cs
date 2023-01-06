namespace SpiritIsland.JaggedEarth;

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit, GameState gameState, UnitOfWork actionScope ) 
		: base( spirit, gameState, actionScope ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}
