using System;
using SpiritIsland.SinglePlayer;

namespace SpiritIslandCmd {
	public class ConsoleLogger : ILogger {
		public void Log( string s ) {
			Console.WriteLine(s);
		}
	}

}
