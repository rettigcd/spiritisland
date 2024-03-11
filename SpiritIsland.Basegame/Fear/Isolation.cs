namespace SpiritIsland.Basegame;

public class Isolation : FearCardBase, IFearCard {

	public const string Name = "Isolation";
	string IOption.Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer/Town from a land where it is the only Invader." )]
	public Task Level1( GameState ctx )
		=> Cmd.RemoveExplorersOrTowns(1)
			.From().SpiritPickedLand().Which( Has.Only1ExplorerTown )
			.ByPickingToken(Human.Explorer_Town)
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player removes 1 Explorer/Town from a land with 2 or fewer Invaders." )]
	public Task Level2( GameState ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Has.TwoOrFewerInvaders )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player removes an Invader from a land with 2 or fewer Invaders." )]
	public Task Level3( GameState ctx )
		=> Cmd.RemoveInvaders(1)
			.From().SpiritPickedLand().Which( Has.TwoOrFewerInvaders )
			.ByPickingToken( Human.Invader )
			.ForEachSpirit()
			.ActAsync( ctx );

}