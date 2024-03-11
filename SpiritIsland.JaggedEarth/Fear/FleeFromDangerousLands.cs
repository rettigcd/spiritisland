namespace SpiritIsland.JaggedEarth;

public class FleeFromDangerousLands : FearCardBase, IFearCard {

	public const string Name = "Flee from Dangerous Lands";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Push 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level1( GameState ctx )
		=> Cmd.PushExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( Has.DangerousLands )
			// .ByPickingToken( Human.Explorer_Town )  This doesn't work because Push is now a Move, not a SpaceToken
			.ForEachBoard()
			.ActAsync( ctx );

	[FearLevel(2, "On Each Board: Remove 1 Explorer/Town from a land with Badlands/Wilds/Dahan." )]
	public Task Level2( GameState ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().OneLandPerBoard().Which( Has.DangerousLands )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachBoard()
			.ActAsync( ctx );

	[FearLevel(3, "On Each Board: Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan." )]
	public Task Level3( GameState ctx )
		=> new SpaceAction( "Remove 1 Explorer/Town from any land, or Remove 1 City from a land with Badlands/Wilds/Dahan.", Level3_Remove)
			.On().OneLandPerBoard()
			.ByPickingToken( ctx => ctx.Space.OfAnyTag( TokensClassesFor(ctx) ) )
			.ForEachBoard()
			.ActAsync(ctx);

	Task Level3_Remove( TargetSpaceCtx ctx ) => ctx.SourceSelector.AddGroup( 1, TokensClassesFor( ctx ) ).RemoveN(ctx.Self);

	static ITokenClass[] TokensClassesFor( TargetSpaceCtx ctx ) => Has.DangerousLands.MyFilter( ctx ) ? Human.Invader : Human.Explorer_Town;

}