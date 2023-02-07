namespace SpiritIsland.JaggedEarth;

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit, GameState gameState ) 
		: base( spirit, gameState ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}
