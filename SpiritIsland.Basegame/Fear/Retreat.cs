namespace SpiritIsland.Basegame;

public class Retreat : FearCardBase, IFearCard {

	public const string Name = "Retreat";
	public string Text => Name;

	[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
	public Task Level1( GameCtx ctx ) {
		return Cmd.PushUpToNExplorers( 2 )
			.From().SpiritPickedLand().Which( Is.Inland )
			.ForEachSpirit()
			.Execute( ctx );
	}

	[FearLevel( 2, "Each player may Push up to 3 Explorer/Town from an Inland land." )]
	public Task Level2( GameCtx ctx ) {
		return Cmd.PushUpToNInvaders( 3, Invader.Explorer_Town )
			.From().SpiritPickedLand().Which( Is.Inland )
			.ForEachSpirit()
			.Execute( ctx );
	}

	[FearLevel( 3, "Each player may Push any number of Explorer/Town from one land." )]
	public Task Level3( GameCtx ctx ) {
		return Cmd.PushUpToNInvaders( int.MaxValue, Invader.Explorer_Town )
			.From().SpiritPickedLand()
			.ForEachSpirit()
			.Execute( ctx );
	}

}