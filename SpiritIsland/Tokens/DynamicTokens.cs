namespace SpiritIsland;

public class DynamicTokens {

	readonly Dictionary<UniqueToken, List<Func<SpaceState, int>>> dict = new Dictionary<UniqueToken, List<Func<SpaceState, int>>>();

	public void Register( System.Func<SpaceState, int> calcCountOnSpace, UniqueToken targetToken ) {
		if(!dict.ContainsKey( targetToken ))
			dict.Add( targetToken, new List<Func<SpaceState, int>>() );
		dict[targetToken].Add( calcCountOnSpace );
	}

	public int GetDynamicTokenFor( SpaceState space, UniqueToken token )
		=> dict.ContainsKey( token ) ? dict[token].Sum( x => x( space ) ) : 0;
	public void Clear() => dict.Clear();

	public virtual IMemento<DynamicTokens> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<DynamicTokens> memento ) {
		((Memento)memento).Restore( this );
	}

	protected class Memento : IMemento<DynamicTokens> {
		public Memento( DynamicTokens src ) {
			dict = src.dict.ToDictionary(p=>p.Key,p=>p.Value); // make copy
		}
		public void Restore( DynamicTokens src ) {
			src.dict.Clear();
			foreach(var p in dict)
				src.dict.Add(p.Key,p.Value);
		}
		readonly Dictionary<UniqueToken, List<Func<SpaceState, int>>> dict = new Dictionary<UniqueToken, List<Func<SpaceState, int>>>();
	}


}