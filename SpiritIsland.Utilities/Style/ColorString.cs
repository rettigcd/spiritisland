using System.Text.RegularExpressions;

namespace SpiritIsland;

public static class ColorString{
	
	/// <summary>
	/// Formats: 
	/// #712 is RGB, 
	/// ###### is RRGGBB, 
	/// ######## is RRGGBBAA,
	/// LightBlue (case sensitive)
	/// </summary>
	static public Color Parse(string htmlColor){
		return ColorTranslator.FromHtml(htmlColor);
	// 	int l = htmlColor.Length;

	// 	if((l==3||l==6||l==8) && Regex.IsMatch(htmlColor,"^[0-9a-fA-F]+$"))
	// 		throw new Exception(htmlColor);

	// 	if(htmlColor[0] == '#')
	// 		return htmlColor.Length switch {
	// 			9 => Color.FromArgb( Parse2Hex(htmlColor,1+6), Parse2Hex(htmlColor,1+0), Parse2Hex(htmlColor,1+2), Parse2Hex(htmlColor,1+4) ),
	// 			7 => Color.FromArgb( Parse2Hex(htmlColor,1+0), Parse2Hex(htmlColor,1+2), Parse2Hex(htmlColor,1+4) ),
	// 			4 => Color.FromArgb( ParseHex(htmlColor[1+0])*17, ParseHex(htmlColor[1+1])*17, ParseHex(htmlColor[1+2]) ),
	// 			_ => throw new ArgumentException(nameof(htmlColor),$"Invalid hex color '{htmlColor}'")
	// 		};
	// 	Color color = Color.FromName(htmlColor);
	// 	if(color.A == 0)
	// 		throw new FormatException($"Unable to find ColorName '{htmlColor}'" );
	// 	return color;
	}
	
	// static int Parse2Hex(string s, int start) => ParseHex(s[start]) * 16 + ParseHex(s[start+1]);
	// static int ParseHex(char k) => 'a'<=k ? (k-'a'+10) : 'A'<=k ? (k-'A'+10) : (k-'0');

}