using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIslandCmd {

	public class ResolveGrowth : IPhase {
		
		readonly Spirit spirit;
		readonly GameState gameState;
		// put speed here

		public ResolveGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
			// put speed here
		}

		public void Initialize() {

			if(spirit.UnresolvedActionFactories.Count == 0) {
				this.Complete?.Invoke();
				return;
			}

			Prompt = GeneratePrompt();
			action = null;
		}

		public string Prompt { get; private set; }

		public event Action Complete;

		public bool Handle( string cmd, int index ) {
			if(index < 0) return false;
			if( action == null ){
				var growthAction = spirit.UnresolvedActionFactories[index];
				this.growthName = growthAction.Name;
				action = growthAction.Bind(spirit,gameState);
				Prompt = GenerateResolutionPrompt();
			} else {
				if(index>=action.Options.Length) return false;
				action.Select(action.Options[index]);
				if(action.IsResolved){
					action.Apply();
					Initialize(); // hack
				} else {
					Prompt = GenerateResolutionPrompt();
				}
			}
			return true;
		}

		string Format(IOption option){
			return option is Track track
					? FormatTrack( track )
				: option is Space space 
					? FormatSpace( space )
				: option.Text;
		}

		string FormatSpace( Space space ) {
			var details = gameState.InvadersOn( space ).ToString();
			int dahanCount = gameState.GetDahanOnSpace( space );
			if(dahanCount > 0)
				details += " D" + dahanCount;
			string pres = spirit.Presence.Where(p=>p==space).Select(x=>"P").Join("");
			return $"{space.Label} {space.Terrain}\t{details}\t{pres}";
		}

		string FormatTrack( Track track ) {
			string details = track == Track.Energy
				? "$/turn = " + spirit.EnergyPerTurn // !!! Display track
				: "Card/turn = " + spirit.NumberOfCardsPlayablePerTurn; // !!! display track
			return $"{track.Text} {details}";
		}


		string GenerateResolutionPrompt() {
			int i = 0;
			return "Select Resolution for " + growthName + " : " + action.Options
				.Select( o => "\r\n" + (++i).ToString() + " : " + Format(o) )
				.Join( "" );
		}

		string GeneratePrompt() {
			int i = 0;
			return "Select Growth to resolve:" + spirit.UnresolvedActionFactories
				.Select( x => "\r\n" + (++i).ToString() + " : " + x.Name)
				.Join( "" );
		}

		IAction action;
		string growthName;
	}

}
