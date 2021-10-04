using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	namespace Decision {

		public class TypedDecision<T> : IDecisionPlus where T:class,IOption {

			public bool AllowAutoSelect { get; }

			public string Prompt { get; }

			public IOption[] Options { get; }

			public TypedDecision(
				string prompt,
				IEnumerable<T> options,
				Present present
			){

				var optionList = options.Cast<IOption>().ToList();
				if(optionList.Count != 0 && present == Present.Done )
					optionList.Add(TextOption.Done);

				Prompt = prompt;
				Options = optionList.ToArray();
				AllowAutoSelect = present != Present.Always;

			}

			public TypedDecision(
				string prompt,
				IEnumerable<T> options,
				string cancelPrompt = null
			){

				var optionList = options.Cast<IOption>().ToList();
				if(optionList.Count != 0 && cancelPrompt != null )
					optionList.Add(new TextOption(cancelPrompt));

				Prompt = prompt;
				Options = optionList.ToArray();
				AllowAutoSelect = false;

			}

			public void Select( IOption option ) => throw new NotImplementedException();
		}

	}

	public enum Present {
		/// <summary> Shows for 1 or more items.</summary>
		Always,

		/// <summary>
		/// Shows for 1 or more items, plus has done, cancel
		/// </summary>
		Done,
	}



}