namespace SpiritIsland;

public class StartUpCounts {

	#region Constructor
	public StartUpCounts(string config){ this.config = config; }
	#endregion

	public bool IsEmpty => string.IsNullOrEmpty( config );
	public int Cities => config.Count(c=>c=='C');
	public int Towns => config.Count(c=>c=='T');
	public int Explorers => config.Count(c=>c=='E');
	public int Dahan => config.Count(c=>c=='D');
	public int Blight => config.Count(c=>c=='B');

	public void Adjust(char k,int delta ) {
		var dict= new CountDictionary<char>();
		for(int i = 0; i < config.Length; ++i)
			dict[config[i]]++;
		dict[k] += delta;
		if(dict[k] < 0) return;
		var buf = new StringBuilder();
		foreach(var kk in dict.Keys)
			buf.Append(kk,dict[kk]);
		config = buf.ToString();
	}

	#region private fields
	string config;
	#endregion
}