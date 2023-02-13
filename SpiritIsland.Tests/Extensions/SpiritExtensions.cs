namespace SpiritIsland.Tests;

public static class SpiritExtensions {

	/// <summary> Binds to the Next Decision </summary>
	internal static DecisionContext NextDecision( this Spirit spirit ) {
		spirit.WaitForNext();
		return new DecisionContext( spirit );
	}

	internal static void WaitForNext( this Spirit spirit ){
		const int ms = 3000;
		if( !spirit.Gateway.WaitForNext(ms) )
			throw new Exception($"Engine did not present Decision withing {ms}");

	}

	internal static SpiritConfigContext Configure( this Spirit spirit ) => new SpiritConfigContext( spirit );

	internal static void Adjust( this SpiritPresence presence, SpaceState space, int count ) => space.Adjust( presence.Token, count );

	internal static List<string> LogAsStrings(this GameState gameState ) {
		var items = new List<string>();
		gameState.NewLogEntry += x => items.Add(x.Msg());
		return items;
	}

	internal static Queue<string> LogInvaderActions(this GameState gameState ) {
		var log = new Queue<string>();
		void RecordLogItem( Log.ILogEntry s ) {
			if(s is Log.InvaderActionEntry or Log.RavageEntry)
				log.Enqueue( s.Msg() );
		}
		gameState.NewLogEntry += RecordLogItem; // (s) => log.Enqueue(s.Msg);
		return log;
	}

}
