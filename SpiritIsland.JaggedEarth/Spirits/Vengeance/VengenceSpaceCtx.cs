namespace SpiritIsland.JaggedEarth;

public class VengenceSpaceCtx : TargetSpaceCtx {
	public VengenceSpaceCtx( VengenceCtx ctx, Space target):base( ctx, target ) { }
	public override TokenBinding Badlands => new WreakVengeanceForTheLandsCorruption( Tokens, ActionCtx );
}
