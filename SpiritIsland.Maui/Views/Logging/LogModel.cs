using SpiritIsland.Log;
using System.Collections.ObjectModel;

namespace SpiritIsland.Maui; 

public class LogModel {

	public ObservableCollection<LogEntryModel> Entries { get; set; } = [];

	public void SpecifyGameConfig( string config) {
		Entries.Insert(0, new LogEntryModel(config,Colors.Gold));
	}

	public void Gs_NewLogEntry(Log.ILogEntry obj) {
		_entries.Add(obj);

		Color c = obj switch {
			DecisionLogEntry => Colors.LightGreen,

			// Bad stuff
			GameOverLogEntry or
			ExceptionEntry or
			IslandBlighted => Colors.Red,

			// Invades
			InvaderActionEntry or
			RavageEntry or
			SpaceExplored => Colors.LightBlue,

			RewindException => Colors.Fuchsia,

			Log.Phase or 
			Round => Colors.Lavender,

			// Debug stuff
			BlightOnCardChanged or
			FearCardRevealed or
			FearGenerated => Colors.LightGray,

			Debug or
			LayoutChanged or 
			TokenMovedArgs or
			TokenReplacedArgs or
			HumanAdjustment => Colors.PeachPuff,

			//Adversary.SetupDescription
			//			CommandBeasts
			//			TokenMovedArgs
			//			TokenReplacedArgs
			//			HumanAdjustment
			_ => Colors.White
		};

		Entries.Add(new LogEntryModel(obj.Msg(Log.LogLevel.Info),c));
	}

	readonly List<Log.ILogEntry> _entries = [];
}

public record LogEntryModel( string text, Color color );