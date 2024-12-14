namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Let's spirit repeat action by paying 1 Time for each previous use.
/// </summary>
class RepeatWithTimeAttribute : RepeatAttribute {

	public int UpTo { get; set; }

	public RepeatWithTimeAttribute() {
		this.UpTo = int.MaxValue;
	}

	public override IDrawableInnateTier[] ThresholdTiers => [];

	public override IPowerRepeater GetRepeater() => new Repeater(UpTo);

	class Repeater( int _max ) : IPowerRepeater {
		int previousUse = 1; // assume when we repeat, we've already used it once.

		public async Task<bool> ShouldRepeat( Spirit spirit ) {

			if( spirit is FracturedDaysSplitTheSky fracturedDays
				&& previousUse <= _max
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