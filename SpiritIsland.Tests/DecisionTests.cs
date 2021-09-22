using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class DecisionTests {

		protected DecisionTests(Spirit spirit ) { 
			this.spirit = spirit; 
			this.User = new VirtualUser(spirit);
		}

		protected readonly Spirit spirit;

		protected VirtualUser User { get; }

	}

}
