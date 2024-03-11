namespace SpiritIsland;

public sealed class DynamicTokens : IHaveMemento {

	readonly Dictionary<ITokenClass, List<Func<Space, int>>> dict = [];

	public void Register( System.Func<Space, int> calcCountOnSpace, ITokenClass targetToken ) {
		if(!dict.ContainsKey( targetToken ))
			dict.Add( targetToken, [] );
		dict[targetToken].Add( calcCountOnSpace );
	}

	public int GetDynamicTokenFor( Space space, TokenClassToken token )
		=> dict.ContainsKey( token ) ? dict[token].Sum( x => x( space ) ) : 0;
	public void Clear() => dict.Clear();

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	class MyMemento( DynamicTokens _src ) {
		public void Restore( DynamicTokens src ) {
			src.dict.Clear();
			foreach(var p in dict)
				src.dict.Add(p.Key,p.Value);
		}
		readonly Dictionary<ITokenClass, List<Func<Space, int>>> dict = _src.dict.ToDictionary( p => p.Key, p => p.Value );
	}


}