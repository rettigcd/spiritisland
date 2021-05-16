using System.Collections.Generic;

namespace SpiritIsland {
	public class DefaultDictionary<K,V> {
		public DefaultDictionary(){}
		public V this[K key]{
			get{ return _dict.ContainsKey(key) ? _dict[key] : default; }
			set{ _dict[key] = value; }
		}
		readonly Dictionary<K,V> _dict = new Dictionary<K, V>(); 
	}

}
