using System;
using System.Linq;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute {

		public InnateOptionAttribute( string elementText, string description, int group=0 ) {
			Elements = ElementList.Parse( elementText );
			ElementText = Elements.BuildElementString( " " );
			Description = description;
			Purpose = AttributePurpose.DisplayAndExecute;
			Group = group;
		}

		/// <summary>
		/// Use this to not trigger the method call. Just display the power to the user
		/// </summary>
		public InnateOptionAttribute( string elementText, string description, AttributePurpose purpose, int group=0 ) {
			Elements = ElementList.Parse( elementText );
			ElementText = Elements.BuildElementString( " " );
			Description = description;
			Purpose = purpose;
			Group = group;
		}

		public string ElementText { get; }

		public string Description { get; }

		public CountDictionary<Element> Elements { get; }

		public AttributePurpose Purpose { get; }

		// Element attributes are evaluated against their group so each group can have 1 method to activate.
		public int Group { get; }

	}

	public enum AttributePurpose { DisplayAndExecute, ExecuteOnly, DisplayOnly }

}