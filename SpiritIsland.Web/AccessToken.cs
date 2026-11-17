using System.Security.Cryptography;
using System.Text;

namespace SpiritIsland.Web;

/// <summary>
/// A super simple shared access token: every request to the API must send it in a header, and everybody uses the same one.
/// There is no user login and the connection is plain HTTP, so this only keeps casual visitors out of a server that other
/// devices on the network can reach (see --lan). Change <see cref="Value"/> to change the token.
/// </summary>
public static class AccessToken {

	public const string Value = "blue32";

	const string StartingValue = "blue32";

	/// <summary> Whether the token has still not been changed from the one it shipped with (the server warns about it when it is reachable from the internet). </summary>
	public static bool IsStillTheStartingValue => Value == StartingValue;

	/// <summary> The request header the client sends it in. </summary>
	public const string HeaderName = "X-Access-Token";

	/// <summary> Compares in constant time, so how long a wrong guess takes says nothing about how close it was. </summary>
	public static bool IsValid( string? candidate )
		=> candidate is not null && CryptographicOperations.FixedTimeEquals( Encoding.UTF8.GetBytes( candidate ), Encoding.UTF8.GetBytes( Value ) );

}
