﻿using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionOption {

		public string Description { get; }
		public bool IsApplicable { get; }

		public Task Action(){
			if( func!=null )
				return func();
			action();
			return Task.CompletedTask;
		}

		readonly Func<Task> func;
		readonly Action action;

		public ActionOption( string description, Action action, bool applicable = true ) {
			Description = description;
			this.action = action;
			IsApplicable = applicable;
		}

		public ActionOption( string description, Func<Task> action, bool applicable = true ) {
			Description = description;
			IsApplicable = applicable;
			func = action;
		}
	}


}