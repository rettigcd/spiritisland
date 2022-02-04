namespace SpiritIsland.JaggedEarth;

public class FleeFromDangerousLands : IFearOptions {

	public const string Name = "Flee from Dangerous Lands";
	string IFearOptions.Name => Name;


	[FearLevel(1, "On Each Board: Push 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
	public Task Level1( FearCtx ctx ) {

		// On Each Board, Push 1 Explorer / Town from a land with Badlands / Wilds / Dahan.
		return Cmd.PushExplorersOrTowns(1)
			.FromLandOnBoard( FindBadlandsWildsOrDahanSpaces,"a land with Badlands / Wilds / Dahan." )
			.OnEachBoard()
			.Execute( ctx.GameState );

	}

	[FearLevel(2, "On Each Board: Remove 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
	public Task Level2( FearCtx ctx ) {

		// On Each Board, Remove 2 Explorer / Town from a land with Badlands / Wilds / Dahan.
		return Cmd.RemoveExplorersOrTowns(2)
			.FromLandOnBoard( FindBadlandsWildsOrDahanSpaces,"a land with Badlands / Wilds / Dahan" )
			.OnEachBoard()
			.Execute( ctx.GameState );
	}

	[FearLevel(3, "On Each Board: Remove 1 Explorer / Town from any land, or Remove 1 City from a land with Badlands / Wilds / Dahan." )]
	public async Task Level3( FearCtx ctx ) {

		// Remove 1 Explorer / Town from any land
		var removeExplorerOrTownFromAnyLand = Cmd.RemoveExplorersOrTowns(1).InAnyLandOnBoard();

		// Remove 1 City from a land with Badlands / Wilds / Dahan.
		var removeCity = Cmd.RemoveCities(1).FromLandOnBoard( FindBadlandsWildsOrDahanSpaces, "a land with Badlands / Wilds / Dahan" );

		await Cmd.Pick1( removeExplorerOrTownFromAnyLand, removeCity )
			.OnEachBoard()
			.Execute( ctx.GameState );

	}

	static bool FindBadlandsWildsOrDahanSpaces( TargetSpaceCtx ctx )
		=>	   ctx.Tokens.Badlands.Any 
			|| ctx.Tokens.Wilds.Any 
			|| ctx.Tokens.Dahan.Any;

}