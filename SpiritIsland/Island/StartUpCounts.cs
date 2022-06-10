namespace SpiritIsland;

public class StartUpCounts {

	#region Constructor
	public StartUpCounts(string config){this.config = config;}
	#endregion

	public bool Empty => string.IsNullOrEmpty( config );
	public int Cities => config.Count(c=>c=='C');
	public int Towns => config.Count(c=>c=='T');
	public int Explorers => config.Count(c=>c=='E');
	public int Dahan => config.Count(c=>c=='D');
	public int Blight => config.Count(c=>c=='B');

	#region private fields
	readonly string config;
	#endregion
}