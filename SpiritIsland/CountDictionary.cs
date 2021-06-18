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

}
