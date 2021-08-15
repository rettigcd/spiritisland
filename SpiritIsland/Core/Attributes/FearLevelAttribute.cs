using System;

namespace SpiritIsland {

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FearLevelAttribute : Attribute {
		public FearLevelAttribute(int _, string _1 ) { }
	}

}

