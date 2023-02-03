namespace SpiritIsland.BranchAndClaw;

public class Discord : FearCardBase, IFearCard {

	public const string Name = "Discord";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife in a different land with at least 2 Invaders." )]
	public Task Level1( GameCtx ctx )
		=> EachPlayerAdd1StrifeInADifferentLand
			.Execute( ctx );

	[FearLevel( 2, "Each player adds 1 Strife in a different land with at least 2 Invaders. Then each Invader takes 1 Damage per Strife it has." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				Cmd.EachStrifeDamagesInvader.In().EachActiveLand()
			).Execute( ctx );

	[FearLevel( 3, "Each player adds 1 Strife in a different land with at least 2 Invaders. Then, each Invader with Strife deals Damage to other Invaders in that land." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.In().EachActiveLand()
			).Execute( ctx );

	static IExecuteOn<GameCtx> EachPlayerAdd1StrifeInADifferentLand 
		=> Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.AtLeastN(2,Human.Invader) ).AllDifferent().ForEachSpirit();
}