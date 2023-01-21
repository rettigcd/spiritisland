namespace SpiritIsland.BranchAndClaw;

public class Discord : FearCardBase, IFearCard {

	public const string Name = "Discord";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 strife in a different land with at least 2 invaders" )]
	public Task Level1( GameCtx ctx )
		=> EachPlayerAdd1StrifeInADifferentLand
			.Execute( ctx );

	[FearLevel( 2, "Each player adds 1 strife in a different land with at least 2 invaders. Then each invader takes 1 damage per strife it has." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				Cmd.EachStrifeDamagesInvader.In().EachActiveLand()
			).Execute( ctx );

	[FearLevel( 3, "each player adds 1 strife in a different land with at least 2 invaders. Then, each invader with strife deals damage to other invaders in that land." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.In().EachActiveLand()
			).Execute( ctx );

	static IExecuteOn<GameCtx> EachPlayerAdd1StrifeInADifferentLand 
		=> Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.TwoOrMoreInvaders ).AllDifferent().ForEachSpirit();
}