using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class CountDictionary<K> {

		#region constructor

		public CountDictionary(){
			_inner = new Dictionary<K, int>();
		}

		public CountDictionary(Dictionary<K,int> inner){
			this._inner = inner;
			foreach(K key in _inner.Keys.ToArray())
				if(_inner[key] == 0)
					_inner.Remove(key);
		}

		#endregion

		public Dictionary<K,int>.KeyCollection Keys => _inner.Keys;

		public bool HasAnyKey( params K[] needles ) => needles.Any( _inner.ContainsKey );

		public void Clear() => _inner.Clear();

		public void AddRange(IEnumerable<K> items) { foreach(var item in items) ++this[item]; }

		public bool Has( Dictionary<K, int> needed ) {
			return needed.All( pair => pair.Value <= this[pair.Key] );
		}

		public bool Has( params K[] requiredElements ) {
			var required = requiredElements
				.GroupBy( x => x )
				.ToDictionary( grp => grp.Key, grp => grp.Count() );
			return Has( required );
		}

		public int this[K key]{
			get{ return _inner.ContainsKey(key) ? _inner[key] : 0; }
			set{ 
				if(value != 0) 
					_inner[key] = value; 
				else if(_inner.ContainsKey(key)) 
					_inner.Remove(key); 
			}
		}
		readonly Dictionary<K,int> _inner; 
	}

	public static class ExtendDictionary {
		static public CountDictionary<T> ToCountDict<T>(this Dictionary<T,int> inner)
			=> new CountDictionary<T>(inner);
	}

}
