namespace SpiritIsland.NatureIncarnate;

public class CivilUnrest : FearCardBase, IFearCard {

	public const string Name = "Civil Unrest";
	public string Text => Name;

	[FearLevel( 1,"On Each Board: Add 1 Strife to a Town/City in a land not matching a Ravage Card." )]
	public override Task Level1( GameState ctx )
		=> Cmd.AddStrifeTo(1,Human.Town_City)
			.To().OneLandPerBoard().Which( Is.NotRavageCardMatch ).ByPickingToken( Human.Town_City )
			.ForEachBoard()
			.ActAsync( ctx );

	[FearLevel( 2, "On Each Board: Add 1 Strife to a Town/City in a land not matching a Ravage Card. Each Invader takes 1 Damage per Strife it has." )]
	public override Task Level2( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrifeTo(1,Human.Town_City)
				.To().OneLandPerBoard().Which( Is.NotRavageCardMatch ).ByPickingToken( Human.Town_City )
				.ForEachBoard(),
			Cmd.EachStrifeDamagesInvader.In().EachActiveLand()
		).ActAsync( ctx );

	[FearLevel( 3, "On Each Board: Add 1 Strife. Each Invader takes 1 Damage per Strife it has." )]
	public override Task Level3( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife(1)
				.To().OneLandPerBoard().ByPickingToken( Human.Invader )
				.ForEachBoard(),
			Cmd.EachStrifeDamagesInvader.In().EachActiveLand()
        ).ActAsync( ctx );

}

