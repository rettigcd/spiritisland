using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	/// <summary>
	/// Let's spirit repeat action by paying 1 Time for each previous use.
	/// </summary>
	class RepeatWithTimeAttribute : RepeatAttribute {

		public int UpTo { get; set; }

		public RepeatWithTimeAttribute() {
			this.UpTo = int.MaxValue;
		}

		public override IDrawableInnateOption[] Thresholds => System.Array.Empty<IDrawableInnateOption>();

		public override IPowerRepeater GetRepeater() => new Repeater(UpTo);

		class Repeater : IPowerRepeater {
			readonly int max;
			int previousUse = 1; // assume when we repeat, we've already used it once.
			public Repeater(int max ) { this.max = max; }
			public async Task<bool> ShouldRepeat( Spirit spirit ) {

				if( spirit is FracturedDaysSplitTheSky fracturedDays
					&& previousUse <= max
					&& previousUse <= fracturedDays.Time
					&& await spirit.UserSelectsFirstText($"Pay {previousUse} Time to repeat power?", "Yes, repeat", "No, thank you.")
				) {
					await fracturedDays.SpendTime( previousUse );
					++previousUse;
					return true;
				}

				return false;
			}
		}

	}

}
