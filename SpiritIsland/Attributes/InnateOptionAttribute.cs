using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute {

		public InnateOptionAttribute( string elementText, string description ) {
			this.ElementText = elementText;
			this.Elements = ElementList.Parse( elementText );
			this.Description = description;
			this.Purpose = AttributePurpose.DisplayAndExecute;
		}

		/// <summary>
		/// Use this to not trigger the method call. Just display the power to the user
		/// </summary>
		public InnateOptionAttribute( string elementText, string description, AttributePurpose purpose ) {
			this.ElementText = elementText;
			this.Elements = ElementList.Parse( elementText );
			this.Description = description;
			this.Purpose = purpose;
		}

		public string ElementText { get; }

		public string Description { get; }

		public Element[] Elements { get; }

		public AttributePurpose Purpose { get; }

	}

	public enum AttributePurpose { DisplayAndExecute, ExecuteOnly, DisplayOnly }

}