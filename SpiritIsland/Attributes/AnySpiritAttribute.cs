using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnySpiritAttribute : Attribute {}


	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnotherSpiritAttribute : AnySpiritAttribute {} // !!! differenciate with above!

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class YourselfAttribute : AnySpiritAttribute {} // !!! differenciate with above!


}
