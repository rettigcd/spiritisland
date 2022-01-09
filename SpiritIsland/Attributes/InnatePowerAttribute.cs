using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class)]
	public class InnatePowerAttribute : System.Attribute {

		public InnatePowerAttribute(string name){
			Name = name;
			GeneralInstructions = "";
		}

		public InnatePowerAttribute(string name,string generalInstructions){
			Name = name;
			GeneralInstructions = generalInstructions;
		}

		public string Name { get; }
		public string GeneralInstructions { get; }

	}

}