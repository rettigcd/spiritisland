namespace SpiritIsland.Basegame;

public class FearOfTheUnseen : IFearCard {

	public const string Name = "Fear of the Unseen";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }


	[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
	public Task Level1( GameCtx fearCtx ) {
		// each player
		return Cmd.EachSpirit(
			// removes 1 explorer / town
			Cmd.RemoveExplorersOrTowns(1)
				// from a land with sacred sites
				.From(x=>x.Presence.IsSelfSacredSite,"with sacred sites")
		).Execute( fearCtx );
	}

	[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
	public Task Level2( GameCtx fearCtx ) {

		// each player
		return Cmd.EachSpirit(
			// removes 1 explorer or town
			Cmd.RemoveExplorersOrTowns( 1 )
				// from a land with presence
				.From( x => x.Presence.IsHere, "with presence" )
		).Execute( fearCtx );
	}

	[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
	public Task Level3( GameCtx ctx ) {

		return Cmd.EachSpirit(
			Cmd.Pick1(
				// removes 1 explorer or town
				Cmd.RemoveExplorersOrTowns( 1 )
					// from a land with presence
					.From( x => x.Presence.IsHere, "with presence" ),
				// remove 1 city
				Cmd.RemoveCities(1)
					// from a alnd with sacred site
					.From( x=> x.Presence.IsSelfSacredSite, "with sacred site")
			)
		).Execute( ctx );

	}

}