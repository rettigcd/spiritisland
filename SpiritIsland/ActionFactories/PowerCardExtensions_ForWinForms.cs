namespace SpiritIsland {
	static public class PowerCardExtensions_ForWinForms {

		// !!! move this into the UI / WebForms project

		public static string GetImageFilename( this PowerCard card ) {
			string filename = card.Name
				.Replace( ',', '_' )
				.Replace( ' ', '_' )
				.Replace( "__", "_" )
				.Replace( "'", "" )
				.Replace( "-", "" )
				.ToLower();
			string cardType = card.PowerType.Text;
			string ns = card.MethodType.Namespace;
			string edition = ns.Contains( "Basegame" ) ? "basegame"
				: ns.Contains( "BranchAndClaw" ) ? "bac"
				: ns.Contains( "PromoPack1" ) ? "bac"  // !!! temporary
				: ns.Contains( "JaggedEarth" ) ? "je"
				: ns;
			return $".\\images\\{edition}\\{cardType}\\{filename}.jpg";
		}

	}

}