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


	class StopBlightEffects : BaseModEntity, IModifyAddingToken {
		public void ModifyAdding( AddingTokenArgs args ) {
			var config = BlightToken.ForThisAction;
			config.ShouldCascade = false;
			config.DestroyPresence = false;
		}
	}
}