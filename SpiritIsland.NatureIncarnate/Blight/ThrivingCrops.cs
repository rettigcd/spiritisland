namespace SpiritIsland.NatureIncarnate;

public sealed class ThrivingCrops : StillHealthyBlightCard {

	public ThrivingCrops():base(
		"Thriving Crops", 
		"Immediately: On Each Board, Build in 3 lands. (Build Actions in lands without Invaders normally build 1 Town.)", 
		2
	) {}

	public override IActOn<GameState> Immediately 
		=> DoBuild
			.In().NDifferentLandsPerBoard(3)
			.ForEachBoard();

	static SpaceAction DoBuild => new SpaceAction("Build", async ctx=>{
		await GameState.Current.InvaderDeck.Build.Engine.TryToDo1Build(ctx.Space);
	});
}