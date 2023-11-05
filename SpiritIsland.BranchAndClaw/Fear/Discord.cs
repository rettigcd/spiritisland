namespace SpiritIsland.BranchAndClaw;

public class Discord : FearCardBase, IFearCard {

	public const string Name = "Discord";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife in a different land with at least 2 Invaders." )]
	public Task Level1( GameCtx ctx )
		=> EachPlayerAdd1StrifeInADifferentLand
			.ActAsync( ctx );

	[FearLevel( 2, "Each player adds 1 Strife in a different land with at least 2 Invaders. Then each Invader takes 1 Damage per Strife it has." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				Cmd.EachStrifeDamagesInvader.In().EachActiveLand()
			).ActAsync( ctx );

	[FearLevel( 3, "Each player adds 1 Strife in a different land with at least 2 Invaders. Then, each Invader with Strife deals Damage to other Invaders in that land." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
				EachPlayerAdd1StrifeInADifferentLand,
				StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.In().EachActiveLand()
			).ActAsync( ctx );

	static IActOn<GameCtx> EachPlayerAdd1StrifeInADifferentLand 
		=> Cmd.AddStrife( 1 )
			.In().SpiritPickedLand().AllDifferent().Which( Has.AtLeastN(2,Human.Invader) )
			.ByPickingToken(Human.Invader)
			.ForEachSpirit();
}