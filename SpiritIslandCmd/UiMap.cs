using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIslandCmd {
	public class UiMap {
		public string Prompt;
		public Dictionary<string,IOption> dict;
		public List<string> descList;
		public UiMap(string prompt,IEnumerable<IOption> list, Formatter formatter){
			Prompt = prompt;

			int pad = 0;
			foreach(var o in list){
				if(o is IActionFactory factory) pad = Math.Max(pad,factory.Name.Length);
			}

			dict = new Dictionary<string, IOption>();
			descList = new List<string>();

			int labelIndex=1;
			foreach(var option in list){
				string key;
				string description;
				if(option is TextOption txt && txt.Text=="Done"){
					key = txt.Text.Substring(0,1).ToLower();
					description = option.Text;
				} else {
					key = (++labelIndex).ToString();
					description = formatter.Format( option, pad );
				}
				dict.Add(key,option);
				descList.Add(key+" : "+description);
			}

		}
		public IOption GetOption(string cmd){
			return dict.ContainsKey(cmd) ? dict[cmd] : null;
		}
		public string ToPrompt() => Prompt + descList.Select( d => "\r\n\t" + d ).Join( "" );
	}

}
