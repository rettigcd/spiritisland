namespace SpiritIsland;

public sealed class DynamicTokens : IHaveMemento {

	readonly Dictionary<ITokenClass, List<Func<Space, int>>> _dict = [];

	public void Register( 
		Func<Space, int> calcCountOnSpace, 
		ITokenClass targetToken 
	) {
		if(!_dict.ContainsKey( targetToken ))
			_dict.Add( targetToken, [] );
		_dict[targetToken].Add( calcCountOnSpace );
	}

	public int GetDynamicTokenFor( Space space, TokenClassToken token )
		=> _dict.ContainsKey( token ) 
			? _dict[token].Sum( x => x( space ) ) 
			: 0;

	public void Clear() => _dict.Clear();

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	class MyMemento( DynamicTokens _src ) {
		public void Restore( DynamicTokens src ) {
			src._dict.Clear();
			foreach(var p in dict)
				src._dict.Add(p.Key,p.Value);
		}
		readonly Dictionary<ITokenClass, List<Func<Space, int>>> dict = _src._dict.ToDictionary( p => p.Key, p => p.Value );
	}


}