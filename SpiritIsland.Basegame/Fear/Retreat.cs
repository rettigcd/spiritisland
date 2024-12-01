namespace SpiritIsland.Basegame;

public class Retreat : FearCardBase, IFearCard {

	public const string Name = "Retreat";
	string IOption.Text => Name;

	[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
	public override Task Level1( GameState ctx ) {
		return Cmd.PushUpToNExplorers( 2 )
			.From().SpiritPickedLand().Which( Is.Inland )
			.ForEachSpirit()
			.ActAsync( ctx );
	}

	[FearLevel( 2, "Each player may Push up to 3 Explorer/Town from an Inland land." )]
	public override Task Level2( GameState ctx ) {
		return Cmd.PushUpToNInvaders( 3, Human.Explorer_Town )
			.From().SpiritPickedLand().Which( Is.Inland )
			.ForEachSpirit()
			.ActAsync( ctx );
	}

	[FearLevel( 3, "Each player may Push any number of Explorer/Town from one land." )]
	public override Task Level3( GameState ctx ) {
		return Cmd.PushUpToNInvaders( int.MaxValue, Human.Explorer_Town )
			.From().SpiritPickedLand()
			.ForEachSpirit()
			.ActAsync( ctx );
	}

}