namespace SpiritIsland.Basegame;

public class BringerCtx : SelfCtx {
	public BringerCtx( Spirit spirit ):base( spirit ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);

	public override SpaceState TokensOn( Space space ) => new TDaTD_ActionTokens( this, space );

}
