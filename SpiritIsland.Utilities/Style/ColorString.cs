using System.Text.RegularExpressions;

namespace SpiritIsland;

public static class ColorString{
	
	/// <summary>
	/// Formats: 
	/// ### is RGB, 
	/// ###### is RRGGBB, 
	/// ######## is RRGGBBAA,
	/// LightBlue (case sensitive)
	/// </summary>
	static public Color Parse(string color){
		int l = color.Length;

		if((l==3||l==6||l==8) && Regex.IsMatch(color,"^[0-9a-fA-F]+$"))
			throw new Exception(color);

		if(color[0] == '#')
			return color.Length switch {
				9 => Color.FromArgb( Parse2Hex(color,1+6), Parse2Hex(color,1+0), Parse2Hex(color,1+2), Parse2Hex(color,1+4) ),
				7 => Color.FromArgb( Parse2Hex(color,1+0), Parse2Hex(color,1+2), Parse2Hex(color,1+4) ),
				4 => Color.FromArgb( ParseHex(color[1+0])*17, ParseHex(color[1+1])*17, ParseHex(color[1+2]) ),
				_ => throw new ArgumentException(nameof(color),$"Invalid hex color '{color}'")
			};
		return Color.FromName(color);
	}
	
	static int Parse2Hex(string s, int start) => ParseHex(s[start]) * 16 + ParseHex(s[start+1]);
	static int ParseHex(char k) => 'a'<=k ? (k-'a'+10) : 'A'<=k ? (k-'A'+10) : (k-'0');

}