﻿using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public abstract class GeneratesContextAttribute : Attribute {
		public abstract Task<object> GetTargetCtx( SpiritGameStateCtx ctx );

		public abstract string RangeText { get; }

		public abstract string TargetFilter { get; }

		public abstract LandOrSpirit LandOrSpirit { get; }

	}

}