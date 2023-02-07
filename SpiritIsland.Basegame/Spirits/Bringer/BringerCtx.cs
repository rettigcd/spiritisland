namespace SpiritIsland.Basegame;

public class BringerCtx : SelfCtx {
	public BringerCtx( Spirit spirit, GameState gs ):base( spirit, gs ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);

	public override ActionableSpaceState TokensOn( Space space ) => new TDaTD_ActionTokens( this, space );

}
