namespace SpiritIsland;

public static class Spirit_Ctx_Helpers {

	static public Task<bool> YouHave( this Spirit spirit, string elementString ) 
		=> spirit.HasElement( ElementStrings.Parse(elementString), "Power Card Threshold" );


}
