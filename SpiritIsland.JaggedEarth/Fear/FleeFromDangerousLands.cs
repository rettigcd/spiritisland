namespace SpiritIsland.JaggedEarth;

public class FleeFromDangerousLands : FearCardBase, IFearCard {

	public const string Name = "Flee from Dangerous Lands";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Push 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.PushExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( HasDangerousLands )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachBoard()
			.Execute( ctx );

	[FearLevel(2, "On Each Board: Remove 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( HasDangerousLands )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachBoard()
			.Execute( ctx );

	[FearLevel(3, "On Each Board: Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan.", Level3_Remove)
			.On().OneLandPerBoard()
			.ByPickingToken( ctx => ctx.Tokens.OfAnyClass( TokensClassesFor(ctx) ) )
			.ForEachBoard()
			.Execute(ctx);

	Task Level3_Remove( TargetSpaceCtx ctx ) => new TokenRemover( ctx ).AddGroup( 1, TokensClassesFor( ctx ) ).RemoveN();

	static TokenClass[] TokensClassesFor( TargetSpaceCtx ctx ) => HasDangerousLandImp( ctx ) ? Human.Invader : Human.Explorer_Town;

	static TargetSpaceCtxFilter HasDangerousLands => new TargetSpaceCtxFilter( "a land with Badlands/Wilds/Dahan.", HasDangerousLandImp );
	static bool HasDangerousLandImp( TargetSpaceCtx ctx ) => ctx.Tokens.Badlands.Any || ctx.Tokens.Wilds.Any || ctx.Tokens.Dahan.Any;


}