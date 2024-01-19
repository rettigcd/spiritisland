using SpiritIsland.Log;

namespace SpiritIsland.Tests;

static public class GameState_Extensions {

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

	static public void IslandWontBlight( this GameState gameState ) => gameState.Tokens[BlightCard.Space].Init(Token.Blight,100);

	/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
	static public void DisableInvaderDeck( this GameState gs ) {
		var nullCard = InvaderCard.Stage1( Terrain.None );
		gs.InitTestInvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
	}

	static public void InitTestInvaderDeck(this GameState gameState, params InvaderCard[] cards ) {
		gameState.InvaderDeck = new InvaderDeck( cards.ToList(), null );// Don't try to inspect unused!
	}

	static public string Msg( this ILogEntry logEntry ) => logEntry.Msg( LogLevel.Info );

	static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
		gameState.Tokens[space].InvaderSummary().ShouldBe( expectedString );
	}

	static public void Assert_DreamingInvaders( this GameState gameState, Space space, string expectedString ) {

		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		var tokens = gameState.Tokens[space];
		string dreamerSummary = tokens.HumanOfTag(TokenCategory.Invader)
			.Where( x => x.HumanClass.Variant == TokenVariant.Dreaming )
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => tokens[invader] + invader.ToString() )
			.Join( "," );
		dreamerSummary.ShouldBe( expectedString );
	}

	static internal GameState Given_InitializedMinorDeck( this GameState gameState ) {
		gameState.MinorCards = new PowerCardDeck( new List<PowerCard>() {
			// 4 random cards good for 1 draw.
			PowerCard.For(typeof(RainOfBlood)),
			PowerCard.For(typeof(Drought)),			// 1st after Shuffle
			PowerCard.For(typeof(LureOfTheUnknown)),
			PowerCard.For(typeof(SteamVents)),
			PowerCard.For(typeof(CallOfTheDahanWays)),
			PowerCard.For(typeof(CallToBloodshed)),	// 2nd after Shuffle
			PowerCard.For(typeof(CallToIsolation)),
			PowerCard.For(typeof(CallToMigrate))
		}, 1 );
		return gameState;
	}

	class StopBlightEffects : BaseModEntity, IModifyAddingToken {
		public void ModifyAdding( AddingTokenArgs args ) {
			var config = BlightToken.ForThisAction;
			config.ShouldCascade = false;
			config.DestroyPresence = false;
		}
	}
}