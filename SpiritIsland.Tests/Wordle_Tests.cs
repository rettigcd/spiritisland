

using System.Text;
using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public
class Wordle_Tests {

	[Fact]
	public void SolveSeries() {

		var searcher = new Searcher();

		// Issue: Gussing RALLY for word BANAL.  The 2nd L is no-match which hides the L in the last slot

		// BANAL
		searcher.AddGuessResult( "aloes", "-----" );
		searcher.AddGuessResult( "print", "--**-" );
		searcher.AddGuessResult( "cupid", "!--!-" );

		//		//! when guessing rally, the second l is saying- there are no Ls.  However we know there is an L

		var (guess, _) = FindBestGuess( searcher.Candidates );
		Console.WriteLine( guess );

	}



	[Theory]
//	[InlineData( "aloes", "elder")]
//	[InlineData( "bleed", "elder" )]
	[InlineData( "clued", "elder" )]
	public void Guess( string guess, string secret ) {
		var guessResult = new GuessResult( guess, secret );
		Console.WriteLine( guessResult );
	}

	/// <summary>
	/// Finds best UNIVERSAL best 1st guess
	/// </summary>
	[Fact]
	public void FindBest1stGuess() {

		var (bestGuess, bestGrouping) = FindBestGuess( Searcher.AllWords );
		int largestGroup = bestGrouping.Values.Max( x=>x.Count );

		System.IO.File.WriteAllText( "C:\\users\\rettigcd\\desktop\\best-first-guess.txt", $"{bestGuess} has {bestGrouping.Count} groups with max size of {largestGroup}" );
		// aloes has 146 groups with max size of 298
	}

	[Fact]
	public void FindBest2ndGuess() {

		var results = new List<string>();
		var results2 = new List<string>();

		var (guess1, bestGrouping) = FindBestGuess( Searcher.AllWords );

		foreach( var pair in bestGrouping ) {
			var (guess2, subGrouping) = FindBestGuess( pair.Value.ToArray() );
			results.Add( $"{pair.Key} {guess2}" );
			results2.Add( $"{guess2} {pair.Key}" );
		}

		results.Add("================");
		results.AddRange( results2 );

		System.IO.File.WriteAllText( "C:\\users\\rettigcd\\desktop\\best-second-guess.txt", results.Join("\r\n") );
		// aloes has 146 groups with max size of 298
	}


	(string, Dictionary<GuessResult, List<string>>) FindBestGuess( string[] candidateWords ) {

		var allWords = Searcher.AllWords;

		// If we only have 1 or 2 candidates, just pick the first one and try it
		if(candidateWords.Length<=2)
			return (candidateWords[0],null);

		// !! if we have 3, see if 1 of 3 can be used to determine the other 2.  If so, use that.

		string bestGuess = "";
		Dictionary<GuessResult, List<string>> bestCounts = null;
		int bestGroupCount = int.MaxValue;

		foreach(var word in allWords) {

			Dictionary<GuessResult, List<string>> counts = BreakIntoGroups( word, candidateWords );

			int maxGroupForThisWord = counts.Values.Max( x => x.Count );
			if(maxGroupForThisWord < bestGroupCount) {
				bestGuess = word;
				bestGroupCount = maxGroupForThisWord;
				bestCounts = counts;
			}
		}

		return (
			bestGuess,
			bestCounts
		);
	}

	static CountDictionary<GuessResult> CountWordsResultingInEachGuessResult( string guess, string[] allWords ) {
		var dict = new CountDictionary<GuessResult>();
		foreach(var word in allWords) {
			var key = new GuessResult( guess, word );
			++dict[key];
		}
		return dict;
	}


	// 1st: apply what we know to narrow the list
	// 2nd: find the word that splits the current list into groups where the largest group is as small as possible.


	//string[] FilterWords( string[] startingWords, string known, string present, string missing ) {
	//	var here = new System.Text.RegularExpressions.Regex( known.Replace( ' ', '.' ) );
	//	var hasChars = new HashSet<char>( present );
	//	var missingChars = new HashSet<char>( missing );

	//	var allWords = System.IO.File.ReadAllLines( "C:\\users\\rettigcd\\desktop\\5-letter-words.txt" );
	//	return startingWords
	//		.Where( (Func<string, bool>)here.IsMatch ) // know location
	//		.Where( x => new HashSet<char>( x ).IsSupersetOf( hasChars ) ) // known letter
	//		.Where( x => x.All( x => !missingChars.Contains( x ) ) )
	//		.ToArray();
	//}

	Dictionary<GuessResult, List<string>> BreakIntoGroups( string guess, string[] allWords ) {
		var dict = new Dictionary<GuessResult, List<string>>();
		foreach(var word in allWords) {
			var key = new GuessResult( guess, word );
			if(dict.ContainsKey(key))
				dict[key].Add(word);
			else
				dict.Add(key, new List<string> { word });
		}
		return dict;
	}

}

class Searcher {

	static readonly public string[] AllWords = System.IO.File.ReadAllLines( "C:\\users\\rettigcd\\desktop\\5-letter-words.txt" );

	public Searcher() {
		known = new char[5];
		excludeFromSpace = new StringBuilder[5];
		for(int i = 0; i <5; ++i)
			excludeFromSpace[i] = new StringBuilder();

		Candidates = AllWords;
	}

	public void AddGuessResult( string guess, string result ) {
		for(int i = 0; i < 5; ++i) {
			switch(result[i]){
				case '!': known[i] = guess[i]; break;
				case '*': excludeFromSpace[i].Append( guess[i] ); mustHave.Add( guess[i] ); break;
				default: for(int j=0;j<5;++j) excludeFromSpace[j].Append( guess[i] ); break;
			}
		}

		// Filter Candidates
		// Alternative - Since we are keeping the previously selected candidates, could just filter on current knowledge instead of accumulated knowledge
		Candidates = Candidates
			.Where( (Func<string, bool>)BuildRegex().IsMatch )
			.Where( x => new HashSet<char>( x ).IsSupersetOf( mustHave ) ) // known letter
			.ToArray();
	}

	public string[] Candidates { get; private set; }

	string[] CalcCandidates() {
		var regex = BuildRegex();
		return AllWords
			.Where( (Func<string, bool>)regex.IsMatch )
			.ToArray();
	}

	Regex BuildRegex() {
		var buf = new StringBuilder();
		for(int i=0;i<5;++i) {
			if(known[i] != default)
				buf.Append(known[i]);
			else if(excludeFromSpace[i].Length > 0) {
				buf.Append("[^");
				buf.Append( excludeFromSpace[i] );
				buf.Append( ']' );
			} else
				buf.Append('.');
			
		}
		return new Regex(buf.ToString());
	}

	readonly char[] known;
	readonly StringBuilder[] excludeFromSpace;
	readonly HashSet<char> mustHave = new HashSet<char>();
}

class GuessResult : IEquatable<GuessResult> {

	public GuessResult( string guess, string actual ) {

		for(int i = 4; i>=0; --i) {
			var k = guess[i];
			var match = (k == actual[i]) ? HERE 
				: actual.Contains( k ) ? ELSEWHERE 
				: NOWHERE;
			hash = hash * 3 + match;
		}

	}

	public override string ToString() {
		var buf = new System.Text.StringBuilder(5);

		int cur = hash;
		for(int i = 0; i < 5; ++i) {
			int remainder = cur % 3;
			buf.Append( remainder switch {
				2 => '!',
				1 => '*',
				_ => '-'
			} );
			cur = (cur-remainder) / 3;
		}
		return buf.ToString();

	}

	public override int GetHashCode() => hash;
	public override bool Equals( object obj ) => Equals( obj as GuessResult );
	public bool Equals( GuessResult other ) => other != null && hash == other.hash;

	const int NOWHERE = 0;
	const int ELSEWHERE = 1;
	const int HERE = 2;

	int hash;
}
