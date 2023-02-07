namespace SpiritIsland.JaggedEarth;

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit ) : base( spirit ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}
