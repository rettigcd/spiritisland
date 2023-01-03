namespace SpiritIsland.JaggedEarth;

public class FleeFromDangerousLands : FearCardBase, IFearCard {

	public const string Name = "Flee from Dangerous Lands";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Push 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
	public Task Level1( GameCtx ctx ) {

		// On Each Board,
		return Cmd.OnEachBoard(
			// Push 1 Explorer / Town
			Cmd.PushExplorersOrTowns(1)
				// from a land with Badlands / Wilds / Dahan.
				.FromLandOnBoard( FindBadlandsWildsOrDahanSpaces,"a land with Badlands / Wilds / Dahan." )
			)
			.Execute( ctx );

	}

	[FearLevel(2, "On Each Board: Remove 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
	public Task Level2( GameCtx ctx ) {

		// On Each Board, 
		return Cmd.OnEachBoard(
			// Remove 1 Explorer / Town 
			Cmd.RemoveExplorersOrTowns( 1 )
				// from a land with Badlands / Wilds / Dahan.
				.FromLandOnBoard( FindBadlandsWildsOrDahanSpaces, "a land with Badlands / Wilds / Dahan" )
			)
			.Execute( ctx );

	}

	[FearLevel(3, "On Each Board: Remove 1 Explorer / Town from any land, or Remove 1 City from a land with Badlands / Wilds / Dahan." )]
	public async Task Level3( GameCtx ctx ) {

		// On Each Board: Remove 1 Explorer / Town from any land, or Remove 1 City from a land with Badlands / Wilds / Dahan.
		await Cmd.OnEachBoard(
			Cmd.Pick1(
				// Remove 1 Explorer / Town from any land
				Cmd.RemoveExplorersOrTowns(1).InAnyLandOnBoard(),
				// or Remove 1 City from a land with Badlands / Wilds / Dahan.
				Cmd.RemoveCities( 1 ).FromLandOnBoard( FindBadlandsWildsOrDahanSpaces, "a land with Badlands / Wilds / Dahan" )
			)
		)
			.Execute( ctx );

	}

	static bool FindBadlandsWildsOrDahanSpaces( TargetSpaceCtx ctx )
		=>	   ctx.Tokens.Badlands.Any 
			|| ctx.Tokens.Wilds.Any 
			|| ctx.Tokens.Dahan.Any;

}