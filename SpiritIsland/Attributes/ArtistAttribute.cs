namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Method )]
public class ArtistAttribute : Attribute {

	public string Artist { get; }

	public ArtistAttribute( string artist ) {
		Artist = artist;
	}

}

public static class Artists {
	public const string AalaaYassin = "Aalaa Yassin";
	public const string CariCorene = "Cari Corene";
	public const string DamonWestenhofer = "Damon Westenhofer";
	public const string DavidMarkiwsky = "David Markiwsky";
	public const string EmilyHancock = "Emily Hancock";
	public const string GrahamStermberg = "Graham Stermberg";
	public const string JasonBehnke = "Jason Behnke";
	public const string JorgeRamos = "Jorge Ramos";
	public const string JoshuaWright = "Joshua Wright";
	public const string KatBirmelin = "Kat Birmelin";
	public const string KatGuevara = "Kat Guevara";
	public const string LoicBelliau = "Loic Belliau";
	public const string LucasDurham = "Lucas Durham";
	public const string MoroRogers = "Moro Rogers";
	public const string NolanNasser = "Nolan Nasser";
	public const string RockyHammer = "Rocky Hammer";
	public const string ShawnDaley = "Shawn Daley";
	public const string ShaneTyree = "Shane Tyree";
	public const string SydniKruger = "Sydni Kruger";
}