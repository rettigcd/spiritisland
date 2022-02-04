namespace SpiritIsland;

public class StartUpCounts {
	readonly string config;
	public bool Empty => string.IsNullOrEmpty(config);
	public StartUpCounts(string config){this.config = config;}
	public int Cities => config.Count(c=>c=='C');
	public int Towns => config.Count(c=>c=='T');
	public int Explorers => config.Count(c=>c=='E');
	public int Dahan => config.Count(c=>c=='D');
	public int Blight => config.Count(c=>c=='B');

}