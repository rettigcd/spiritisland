namespace SpiritIsland;

public class StartUpCounts( string _config ) {

	public bool IsEmpty => string.IsNullOrEmpty( _config );
	public int Cities    => _config.Count(c=>c=='C');
	public int Towns     => _config.Count(c=>c=='T');
	public int Explorers => _config.Count(c=>c=='E');
	public int Dahan     => _config.Count(c=>c=='D');
	public int Blight    => _config.Count(c=>c=='B');

	#region private fields
	#endregion
}