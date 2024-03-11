namespace SpiritIsland.Basegame;

public class FearOfTheUnseen : FearCardBase, IFearCard {

	public const string Name = "Fear of the Unseen";
	string IOption.Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer/Town from a land with Sacred Site." )]
	public Task Level1( GameState fearCtx ) {
		return Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Has.MySacredSite )
			.ByPickingToken(Human.Explorer_Town)
			.ForEachSpirit()
			.ActAsync( fearCtx );
	}

	[FearLevel( 2, "Each player removes 1 Explorer/Town from a land with Presence." )]
	public Task Level2( GameState fearCtx ) {
		return Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Has.YourPresence )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
			.ActAsync( fearCtx );
	}

	[FearLevel( 3, "Each player removes 1 Explorer/Town from a land with Presence, or 1 City from a land with Sacred Site." )]
	public Task Level3( GameState ctx ) {
		return Cmd.Pick1(  // !! this could be flattened
				Cmd.RemoveExplorersOrTowns( 1 ).From().SpiritPickedLand().Which( Has.YourPresence ),
				Cmd.RemoveCities( 1 ).From().SpiritPickedLand().Which( Has.MySacredSite )
			)
			.ForEachSpirit()
			.ActAsync( ctx );
	}

}