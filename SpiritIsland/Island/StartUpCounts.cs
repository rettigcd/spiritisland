namespace SpiritIsland;

public class StartUpCounts {

	#region Constructor
	public StartUpCounts(string config){ this._config = config; }
	#endregion

	public bool IsEmpty => string.IsNullOrEmpty( _config );
	public int Cities    => _config.Count(c=>c=='C');
	public int Towns     => _config.Count(c=>c=='T');
	public int Explorers => _config.Count(c=>c=='E');
	public int Dahan     => _config.Count(c=>c=='D');
	public int Blight    => _config.Count(c=>c=='B');

	//public void Adjust( char k,int delta ) {
	//	var dict= new CountDictionary<char>();
	//	for(int i = 0; i < config.Length; ++i)
	//		dict[config[i]]++;
	//	dict[k] += delta;
	//	if(dict[k] < 0) return;
	//	var buf = new StringBuilder();
	//	foreach(var kk in dict.Keys)
	//		buf.Append(kk,dict[kk]);
	//	config = buf.ToString();
	//}

	#region private fields
	readonly string _config;
	#endregion
}