
using System;

namespace SpiritIsland {

	/// <summary>
	/// The details to present to the user
	/// </summary>
	public interface IDecision {
		public string Prompt { get; }
		public IOption[] Options { get; }
	}

	/// <summary>
	/// plus the callback when user makes a decision
	/// </summary>
	public interface IDecisionMaker : IDecision {
		public void Select(IOption option);
	}

	public interface IDecisionMakerPlus : IDecisionMaker {
		public bool AllowAutoSelect { get; }
	}

	/// <summary>
	/// Provides a single point to process a stream of decisions / choices
	/// </summary>
	public interface IDecisionStream {

		public IDecision Current { get; }

		public void Choose( IOption option );

		public bool IsResolved { get; }
	}

	/// <summary>
	/// A decision / choice that needs to be made
	/// </summary>
	class Decision : IDecisionMakerPlus {
		public string Prompt {get; set; }

		public IOption[] Options {get;set; }

		public bool AllowAutoSelect => false;

		static public readonly Decision Null = new Decision { Prompt = "-", Options = Array.Empty<IOption>() };

		public void Select( IOption option ) { throw new NotImplementedException(); }
	}

}
