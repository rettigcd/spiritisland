using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class ConfigureRavage {

		public bool ShouldDamageLand { get; set; } = true;
		public bool ShouldDamageDahan { get; set; } = true;

		public int DahanHitpoints { get; set; } = 2;

		public CountDictionary<Token> NotParticipating {  get; set; } = new CountDictionary<Token>();

		public Func<RavageEngine, Task> RavageSequence = null; // null triggers default
	}


}
