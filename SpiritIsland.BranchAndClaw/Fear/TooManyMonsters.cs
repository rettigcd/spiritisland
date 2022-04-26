namespace SpiritIsland.BranchAndClaw;

public class TooManyMonsters : IFearOptions {
	public const string Name = "Too Many Monsters";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player removes 1 explorer / town from a land with beast." )]
	public async Task Level1( FearCtx ctx ) {

		// Each player removes 1 explorer / town from a land with beast.
		foreach(var spiritCtx in ctx.Spirits)
			await spiritCtx.RemoveTokenFromOneSpace( ctx.LandsWithBeasts(), 1, Invader.Explorer );

	}

	[FearLevel( 2, "Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast" )]
	public async Task Level2( FearCtx ctx ) {

		// Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast
		foreach(var spirit in ctx.Spirits)
			await RemoveTokenChoice( spirit, 1, Invader.Explorer );

	}

	[FearLevel( 3, "Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast" )]
	public async Task Level3( FearCtx ctx ) {

		// Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast
		foreach(var spirit in ctx.Spirits)
			await RemoveTokenChoice( spirit, 2, Invader.Explorer, Invader.Town );
	}

	static Task RemoveTokenChoice( SelfCtx ctx, int count, params TokenClass[] interiorGroup ) {

		// !! It would be easier on the player if we could 'flatten' this to just the 'remove' step so they don't have to analyze what is on a beast space or adjacent to one.

		var landsWithBeasts = ctx.GameState.Island.AllSpaces
			.Where( s => ctx.GameState.Tokens[s].Beasts.Any )
			.ToArray();

		return ctx.SelectActionOption(

			new SelfAction("Remove 1 explorer & 1 town from a land with beast", spiritCtx => { 
				return spiritCtx.RemoveTokenFromOneSpace( landsWithBeasts, count, Invader.Explorer, Invader.Town );
			}),

			new SelfAction("Remove 1 "+ interiorGroup.Select(x=>x.Label).Join("/") +" from a land adjacent to beast", spiritCtx => {
				var spaceOptions = landsWithBeasts
					.SelectMany( s=>s.Adjacent )
					.Distinct()
					.Where( s => spiritCtx.GameState.Tokens[s].HasAny( interiorGroup ) );

				return spiritCtx.RemoveTokenFromOneSpace( spaceOptions, 1, interiorGroup );
			})
		);
	}

}