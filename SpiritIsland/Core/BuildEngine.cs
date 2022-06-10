namespace SpiritIsland;

public class BuildEngine {

	protected TokenCountDictionary tokens;
	protected BuildingEventArgs.BuildType buildType;
	protected GameState gameState;

	protected readonly Guid actionId = Guid.NewGuid();

	public BuildEngine() {}

	public async Task<string> Exec( 
		BuildingEventArgs args,
		TokenCountDictionary tokens, 
		GameState gameState 
	) {
		if( !tokens.HasInvaders() ) return "No invaders";

		BuildingEventArgs.BuildType buildType = args.GetBuildType( tokens.Space );

		this.tokens = tokens;
		this.buildType = buildType;
		this.gameState = gameState;

		if( await StopBuildWithDiseaseBehavior() )
			return tokens.Space.Label +" build stopped by disease";

		// Determine type to build
		int townCount = tokens.Sum( Invader.Town );
		int cityCount = tokens.Sum( Invader.City );
		HealthTokenClass invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;

		// check if we should
		bool shouldBuild = buildType switch {
			BuildingEventArgs.BuildType.CitiesOnly => invaderToAdd == Invader.City,
			BuildingEventArgs.BuildType.TownsOnly => invaderToAdd == Invader.Town,
			_ => true,
		};
		// build it
		if(shouldBuild) {
			await tokens.AddDefault( invaderToAdd, 1, actionId, AddReason.Build );
		}

		return invaderToAdd.Label;
	}

	virtual protected async Task<bool> StopBuildWithDiseaseBehavior() {
		var disease = tokens.Disease;
		bool stoppedByDisease = disease.Any;
		if(stoppedByDisease)
			await disease.Bind(actionId).Remove(1, RemoveReason.UsedUp);

		return stoppedByDisease;
	}

}
