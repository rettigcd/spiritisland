using SpiritIsland.Log;

namespace SpiritIsland.Tests;

static public class GameState_Extensions {

	/// <summary>Wipes all invaders off of the island. </summary>
	internal static GameState Given_InvadersDisappear( this GameState gs ){
		foreach(Space space in ActionScope.Current.Spaces_Unfiltered)
			space.Given_ClearInvaders();
			return gs;
	}

	internal static List<string> LogAsStringList( this GameState gameState ) {
		var items = new List<string>();
		gameState.NewLogEntry += x => items.Add( x.Msg() );
		return items;
	}

	internal static Queue<string> LogInvaderActions( this GameState gameState ) {
		var log = new Queue<string>();
		void RecordLogItem( Log.ILogEntry s ) {
			if(s is Log.InvaderActionEntry or Log.RavageEntry)
				log.Enqueue( s.Msg() );
		}
		gameState.NewLogEntry += RecordLogItem; // (s) => log.Enqueue(s.Msg);
		return log;
	}

	internal static void DisableBlightEffect( this GameState gs ) {
		gs.AddIslandMod(new StopBlightEffects());
	}

	static public void IslandWontBlight( this GameState gameState ) => gameState.BlightCard.InitBlight(100);

	/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
	static public void DisableInvaderDeck( this GameState gs ) {
		var nullCard = InvaderCard.Stage1( Terrain.None );
		gs.InitTestInvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
	}

	static public void InitTestInvaderDeck(this GameState gameState, params InvaderCard[] cards ) {
		gameState.InvaderDeck = new InvaderDeck( [.. cards], null );// Don't try to inspect unused!
	}

	static public string Msg( this ILogEntry logEntry ) => logEntry.Msg( LogLevel.Info );

	static internal GameState Given_InitializedMinorDeck( this GameState gameState ) {
		gameState.MinorCards = new PowerCardDeck( [
			// 4 random cards good for 1 draw.
			PowerCard.ForDecorated(RainOfBlood.ActAsync),
			PowerCard.ForDecorated(LureOfTheUnknown.ActAsync),
			PowerCard.ForDecorated(SteamVents.ActAsync),
			PowerCard.ForDecorated(CallOfTheDahanWays.ActAsync),
			PowerCard.ForDecorated(CallToBloodshed.ActAsync),	
			PowerCard.ForDecorated(CallToIsolation.ActAsync), // 2nd after Shuffle
			PowerCard.ForDecorated(CallToMigrate.ActAsync),
			PowerCard.ForDecorated(Drought.ActAsync),			// 1st after Shuffle
		], 1 );
		return gameState;
	}

	class StopBlightEffects : BaseModEntity, IModifyAddingToken {
		public Task ModifyAddingAsync( AddingTokenArgs args ) {
			var config = BlightToken.ScopeConfig;
			config.ShouldCascade = false;
			config.DestroyPresence = false;
			return Task.CompletedTask;
		}
	}

	// usage: (await waitForBlightedIsland).Card.Text.ShouldBe("Promising Farmlands");
	static public Task<IslandBlighted> WatchForBlightedIsland(this GameState gs) {
		var blightedSource = new TaskCompletionSource<IslandBlighted>();
		gs.NewLogEntry += (entry) => { 
			if (entry is IslandBlighted ib) 
				blightedSource.TrySetResult(ib); };
		return blightedSource.Task;
	}

	/// <summary>
	/// Resolves when a FearCard is revealed.
	/// </summary>
	static public Task<FearCardRevealed> WatchForFearCard(this GameState gs) {
		var fearSource = new TaskCompletionSource<FearCardRevealed>();
		gs.NewLogEntry += (entry) => {
			if (entry is FearCardRevealed fcr)
				fearSource.TrySetResult(fcr);
		};
		return fearSource.Task;
	}

}