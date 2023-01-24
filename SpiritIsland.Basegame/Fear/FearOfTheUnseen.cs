namespace SpiritIsland.Basegame;

public class FearOfTheUnseen : FearCardBase, IFearCard {

	public const string Name = "Fear of the Unseen";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
	public Task Level1( GameCtx fearCtx ) {
		return Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Has.MySacredSite )
			.ByPickingToken(Invader.Explorer_Town)
			.ForEachSpirit()
			.Execute( fearCtx );
	}

	[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
	public Task Level2( GameCtx fearCtx ) {
		return Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Has.YourPresence )
			.ByPickingToken( Invader.Explorer_Town )
			.ForEachSpirit()
			.Execute( fearCtx );
	}

	[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
	public Task Level3( GameCtx ctx ) {
		return Cmd.Pick1(  // !! this could be flattened
				Cmd.RemoveExplorersOrTowns( 1 ).From().SpiritPickedLand().Which( Has.YourPresence ),
				Cmd.RemoveCities( 1 ).From().SpiritPickedLand().Which( Has.MySacredSite )
			)
			.ForEachSpirit()
			.Execute( ctx );
	}

}