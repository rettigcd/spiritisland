using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class ConfigureRavage {

		public bool ShouldDamageLand { get; set; } = true;
		public bool ShouldDamageDahan { get; set; } = true;

		public int DahanHitpoints { get; set; } = 2;

		public CountDictionary<Token> NotParticipating {  get; set; } = new CountDictionary<Token>();

		public Func<RavageEngine, Task> RavageSequence = null; // null triggers default

		public Func<DahanGroupBinding,int,int,Task> DestroyDahan = DefaultDestroyDahan;

		public Func<Token,int> DamageFromInvader = DefaultDamageFromInvader;

		static async Task DefaultDestroyDahan( DahanGroupBinding dahan, int count, int health ) {
			if(count<=0) return;
			await dahan.Destroy( count, health, Cause.Invaders );
		}

		static int DefaultDamageFromInvader( Token invader ) => invader.FullHealth;

	}

}
