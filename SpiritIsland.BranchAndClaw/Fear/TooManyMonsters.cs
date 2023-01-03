namespace SpiritIsland.BranchAndClaw;

public class TooManyMonsters : FearCardBase, IFearCard {

	public const string Name = "Too Many Monsters";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 explorer / town from a land with beast." )]
	public async Task Level1( GameCtx ctx ) {

		// Each player removes 1 explorer / town from a land with beast.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.RemoveTokenFromOneSpace( ctx.LandsWithBeasts().Select(x=>x.Space), 1, Invader.Explorer );

	}

	[FearLevel( 2, "Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast" )]
	public async Task Level2( GameCtx ctx ) {

		// Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast
		foreach(var spirit in ctx.Spirits)
			await RemoveTokenChoice( spirit, 1, Invader.Explorer );

	}

	[FearLevel( 3, "Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast" )]
	public async Task Level3( GameCtx ctx ) {

		// Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast
		foreach(var spirit in ctx.Spirits)
			await RemoveTokenChoice( spirit, 2, Invader.Explorer, Invader.Town );
	}

	static Task RemoveTokenChoice( SelfCtx ctx, int countToRemoveFromBeastSpace, params TokenClass[] interiorGroup ) {

		// !! It would be easier on the player if we could 'flatten' this to just the 'remove' step so they don't have to analyze what is on a beast space or adjacent to one.

		var landsWithBeasts = ctx.GameState.AllActiveSpaces
			.Where( s => s.Beasts.Any )
			.ToArray();

		return ctx.SelectActionOption(

			new SelfAction($"Remove {countToRemoveFromBeastSpace} explorer(s) & {countToRemoveFromBeastSpace} town(s) from a land with beast", spiritCtx => { 
				return spiritCtx.RemoveTokenFromOneSpace( landsWithBeasts.Select( s => s.Space ), countToRemoveFromBeastSpace, Invader.Explorer, Invader.Town );
			}),

			new SelfAction("Remove 1 "+ interiorGroup.Select(x=>x.Label).Join("/") +" from a land adjacent to beast", spiritCtx => {
				var spaceOptions = landsWithBeasts
					.SelectMany( s=>s.Adjacent )
					.Distinct()
					.Where( s => s.HasAny( interiorGroup ) );

				return spiritCtx.RemoveTokenFromOneSpace( spaceOptions.Select(s=>s.Space), 1, interiorGroup );
			})
		);
	}

}