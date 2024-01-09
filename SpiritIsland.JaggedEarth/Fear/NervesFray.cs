namespace SpiritIsland.JaggedEarth;

public class NervesFray : FearCardBase, IFearCard {

	public const string Name = "Nerves Fray";
	public string Text => Name;

	[FearLevel(1, "Each player adds 1 Strife in a land not matching a Ravage Card." )]
	public Task Level1( GameState ctx )
		=> Cmd.AddStrife( 1 )
		.To().SpiritPickedLand().Which( Is.NotRavageCardMatch )
		.ForEachSpirit()
		.ActAsync( ctx );

	[FearLevel(2, "Each player adds 2 Strife in a single land not matching a Ravage Card." )]
	public Task Level2( GameState ctx )
		=> Cmd.AddStrife( 2 )
		.To().SpiritPickedLand().Which( Is.NotRavageCardMatch )
		.ForEachSpirit()
		.ActAsync( ctx );


	[FearLevel(3, "Each player adds 2 Strife in a single land not matching a Ravage Card. 1 Fear per player." )]
	public async Task Level3( GameState ctx ) {

		await Cmd.AddStrife( 2 )
			.To().SpiritPickedLand().Which( Is.NotRavageCardMatch )
			.ForEachSpirit()
			.ActAsync( ctx );

		// 1 Fear per player.
		ctx.Fear.Add( ctx.Spirits.Length );
	}

}