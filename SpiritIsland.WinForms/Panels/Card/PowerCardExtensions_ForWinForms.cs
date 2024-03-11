namespace SpiritIsland;

static public class PowerCardExtensions_ForWinForms {

	public static string GetImageFilename( this PowerCard card ) {
		string filename = card.Title
			.Replace( ',', '_' )
			.Replace( ' ', '_' )
			.Replace( "__", "_" )
			.Replace( "'", "" )
			.Replace( "-", "" )
			.ToLower();
		string cardType = card.PowerType.Name;
		string ns = card.MethodType.Namespace;
		string edition = ns.Contains( "Basegame" ) ? "basegame"
			: ns.Contains( "BranchAndClaw" ) ? "bac"
			: ns.Contains( "FeatherAndFlame" ) ? "faf"
			: ns.Contains( "JaggedEarth" ) ? "je"
			: ns;
		return $".\\images\\{edition}\\{cardType}\\{filename}.jpg";
	}

}
