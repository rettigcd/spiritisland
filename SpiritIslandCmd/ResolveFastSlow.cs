using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIslandCmd {

	class ResolveFastSlow : IPhase {
		
		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;

		public ResolveFastSlow(Spirit spirit,GameState gameState,Speed speed){
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
		}

		public void Initialize() {
			options = FindMatchingCards();

			if(options.Count == 0) {
				Done();
				return;
			}

			Prompt = SummarizeActions();
			action = null;
		}

		public string Prompt { get; private set; }

		public event Action Complete;

		public bool Handle( string cmd,int index ) {
			if(cmd=="d"){
				Done();
				return true;
			}
			if(index < 0) return false;
			if( action == null )
				SelectAction( index );
			else
				SelectOption( index );
			return true;
		}

		List<IActionFactory> FindMatchingCards() {
			return spirit.UnresolvedActionFactories
				.Where( x => x.Speed == speed )
				.ToList();
		}


		void Done() {

			foreach(var unused in FindMatchingCards())
				spirit.MarkResolved(unused);

			this.Complete?.Invoke();
		}

		void SelectOption( int index ) {
			if(index>=action.Options.Length) return;
			action.Select( action.Options[index] );
			if(action.IsResolved) {
				action.Apply();
				spirit.MarkResolved( growthAction ); // growth actions auto-do this.
				Initialize(); // Next
			} else
				Prompt = SummarizeOptions();
		}

		void SelectAction( int index ) {
			if(index>=options.Count) return;
			growthAction = options[index];
			this.growthName = growthAction.ToString();
			action = growthAction.Bind( spirit, gameState );
			if(action.IsResolved){
				action.Apply();
				spirit.MarkResolved( growthAction ); // growth actions auto-do this.
				Initialize(); // Next
			} else
				Prompt = SummarizeOptions();
		}

		IActionFactory growthAction;

		string SummarizeActions() {
			int i = 0;

			var optionStrings = options
				.Select( x => "\r\n" + (++i).ToString() + " : " + x.Name )
				.ToList();

			optionStrings.Add( "\r\nd : done" );

			return "Select "+speed+" to resolve:" + optionStrings.Join( "" );
		}

		string SummarizeOptions() {

			int i = 0;
			return "Select Resolution for " + growthName + " : " + action.Options
				.Select( o => "\r\n" + (++i).ToString() + " : " + Format(o) )
				.Join( "" );
		}

		string Format(IOption option){
			return option is Space space
					? FormatSpace(space)
				:option.Text;
		}

		string FormatSpace( Space space ) {
			var details = gameState.InvadersOn( space ).ToString();
			int dahanCount = gameState.GetDahanOnSpace( space );
			if(dahanCount > 0)
				details += " D" + dahanCount;
			string pres = spirit.Presence.Where(p=>p==space).Select(x=>"P").Join("");
			return $"{space.Label} {space.Terrain}\t{details}\t{pres}";
		}


		IAction action;
		string growthName;
		List<IActionFactory> options;
	}
}
