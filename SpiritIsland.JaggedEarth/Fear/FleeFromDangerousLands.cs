namespace SpiritIsland.JaggedEarth;

public class FleeFromDangerousLands : FearCardBase, IFearCard {

	public const string Name = "Flee from Dangerous Lands";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Push 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.PushExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( Has.DangerousLands )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachBoard()
			.Execute( ctx );

	[FearLevel(2, "On Each Board: Remove 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( Has.DangerousLands )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachBoard()
			.Execute( ctx );

	[FearLevel(3, "On Each Board: Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceCmd( "Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan.", Level3_Remove)
			.On().OneLandPerBoard()
			.ByPickingToken( ctx => ctx.Tokens.OfAnyClass( TokensClassesFor(ctx) ) )
			.ForEachBoard()
			.Execute(ctx);

	Task Level3_Remove( TargetSpaceCtx ctx ) => new TokenRemover( ctx ).AddGroup( 1, TokensClassesFor( ctx ) ).RemoveN();

	static IEntityClass[] TokensClassesFor( TargetSpaceCtx ctx ) => Has.DangerousLands.Filter( ctx ) ? Human.Invader : Human.Explorer_Town;

}