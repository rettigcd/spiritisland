using System.Text.RegularExpressions;

namespace SpiritIsland.WinForms;

public static class TokenParser {
	public static string[] Tokenize( string s ) {

		var tokens = new Regex( "\\b(sacred site|destroyedpresence|presence|fast|slow"
			+ "|dahan|blight|fear|city|town|explorer"
			+ "|sun|moon|air|fire|water|plant|animal|earth"
			+ "|wetland|jungle|mountain|sands?"
			+ "|beasts?|disease|strife|wilds|badlands|vitality|quake"
			+ "|\\+1range|-or-|incarna|endless-dark|cardplay|impending)\\b", RegexOptions.IgnoreCase
		).Matches( s ).Cast<Match>().ToList();

		var results = new List<string>();

		int cur = 0;
		while(cur < s.Length) {
			// no more tokens, go to the end
			if(tokens.Count == 0) {
				results.Add( s[cur..] );
				break;
			}
			var nextToken = tokens[0];
			if(nextToken.Index == cur) {
				// Add this token to the results
				string tokenText = "{" + nextToken.Value.ToLower() + "}";
				switch(tokenText) {
					case "{beasts}": results.Add( "{beast}" ); break;
                    case "{-or-}":
						results.Add("{or-curly-before}");
						results.Add( " or " );
						results.Add( "{or-curly-after}" );
						break;
                    default: results.Add(tokenText); break;
				}
				// next
				cur = nextToken.Index + nextToken.Length;
				tokens.RemoveAt( 0 );
			} else {
				// Add strings to the results
				results.Add( s[cur..nextToken.Index] );
				cur = nextToken.Index;
			}
		}
		return results.ToArray();
	}

}
