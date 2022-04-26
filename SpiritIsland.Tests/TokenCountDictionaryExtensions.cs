namespace SpiritIsland.Tests;

public static class TokenCountDictionaryExtensions {

	static public string InvaderSummary( this TokenCountDictionary dict ) {

		// !!! Depreate this.  Use .Invaders (to get just the invaders) then .Summary
		static int Order_CitiesTownsExplorers( Token invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return dict.Invaders()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => dict.counts[invader] + invader.ToString() )
			.Join( "," );
	}

}
