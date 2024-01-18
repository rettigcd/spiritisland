using System.Drawing;

namespace SpiritIsland;

static class ColorString{
	static public Color ParseHexColor(string color) => Color.FromArgb( Parse2Hex(color,0), Parse2Hex(color,2), Parse2Hex(color,4) );
	static int Parse2Hex(string s, int start) => ParseHex(s[start]) * 16 + ParseHex(s[start+1]);
	static int ParseHex(char k) => 'a'<=k ? (k-'a'+10) : 'A'<=k ? (k-'A'+10) : (k-'0');

}