using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// Configures Dahan and Invader behavior on a per-space bases.
	/// </summary>
	public class ConfigureRavage {

		// Ravage - Specific
		public Func<RavageAction, Task> RavageSequence = null; // null triggers default
		public bool ShouldDamageLand { get; set; } = true;
		public CountDictionary<Token> NotParticipating {  get; set; } = new CountDictionary<Token>();
		public Func<Token,bool> IsAttacker { get; set; } = null;
		public Func<Token,bool> IsDefender { get; set; } = null;

		// Dahan Behavior
		// !!! Maybe this should be put on the DahanBinding object so it is effective outside ravage
		public Func<DahanGroupBinding,int,int,Task> DestroyDahan = DefaultDestroyDahan;
		public bool ShouldDamageDahan { get; set; } = true;
		public int DahanHitpoints { get; set; } = 2;
		static async Task DefaultDestroyDahan( DahanGroupBinding dahan, int count, int health ) {
			if(count<=0) return;
			await dahan.Destroy( count, health, Cause.Invaders );
		}

		// Invader Behavior
		// !!! Maybe this should be put on the InvaderBinding object so that it is effective outside ravage.
		public Func<Token,int> DamageFromAttacker = DefaultDamageFromInvader;
		static int DefaultDamageFromInvader( Token invader ) => invader.FullHealth;

	}

}
