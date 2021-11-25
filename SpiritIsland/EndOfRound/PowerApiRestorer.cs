﻿using System.Threading.Tasks;

namespace SpiritIsland {
	public class PowerApiRestorer {

		readonly Spirit spirit;
		readonly ICalcRange original;
		public PowerApiRestorer(Spirit spirit ) {
			this.spirit = spirit;
			this.original = spirit.RangeCalc; // capture so we can put it back later
		}
		public Task Restore( GameState _ ) {
			spirit.RangeCalc = original;
			return Task.CompletedTask;
		}

	}

	public class SourceCalcRestorer {

		readonly Spirit spirit;
		readonly ICalcSource original;
		public SourceCalcRestorer(Spirit spirit ) {
			this.spirit = spirit;
			this.original = spirit.SourceCalc; // capture so we can put it back later
		}
		public Task Restore( GameState _ ) {
			spirit.SourceCalc = original;
			return Task.CompletedTask;
		}

	}

	public class RangeCalcRestorer {

		readonly Spirit spirit;
		readonly ICalcRange original;
		public RangeCalcRestorer(Spirit spirit ) {
			this.spirit = spirit;
			this.original = spirit.RangeCalc; // capture so we can put it back later
		}
		public Task Restore( GameState _ ) {
			spirit.RangeCalc = original;
			return Task.CompletedTask;
		}

	}


}
