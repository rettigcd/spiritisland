using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Outfish.JavaScript;

public static class Json {
	static public dynamic    Deserialize( string json )                => new JsonDeserializer( json ).Deserialize();
	static public JsonArray  DeserializeArray( string json )           => new JsonDeserializer( json ).DeserializeArray();
	static public string     DeserializeString( string json )          => new JsonDeserializer( json ).DeserializeString();
	static public JsonObject DeserializeDictionary( string json )      => new JsonDeserializer( json ).DeserializeDictionary();
	static public bool       DeserializeBoolean( string json )         => new JsonDeserializer( json ).DeserializeBoolean( false ).Value;
	static public double     DeserializeDouble( string json )          => new JsonDeserializer( json ).DeserializeNumber( false ).Value;
	static public bool?      DeserializeNullableBoolean( string json ) => new JsonDeserializer( json ).DeserializeBoolean( true );
	static public double?    DeserializeNullableNumber( string json )  => new JsonDeserializer( json ).DeserializeNumber( true );


	static public string Serialize( object obj, int? tabMax=null )                  => new JsonSerializer( tabMax ).Serialize( obj );
	static public string SerializeArray( IEnumerable enumerable, int? tabMax=null ) => new JsonSerializer( tabMax ).SerializeArray( enumerable );
	static public string SerializeDictionary( IDictionary dict, int? tabMax=null )  => new JsonSerializer( tabMax ).SerializeDictionary( dict );

	static public string SerializeBoolean( bool b )       => JsonSerializer.SerializeBoolean( b );
	static public string SerializeString( string s )      => JsonSerializer.SerializeString( s );
	static public string SerializeNumber( object number ) => JsonSerializer.SerializeNumber( number );
}

internal class JsonSerializer {

	#region constructor

	/// <param name="tabMax">Causes the top [tabMax] objects to appear on new lines</param>
	public JsonSerializer( int? tabMax ) {
		_useBreakers = tabMax.HasValue;
		_tabMax = tabMax ?? 0;
	}

	#endregion

	#region public Dynamic Types

	public string Serialize( object obj )	{
		return obj switch {
			null or DBNull    => "null",
			string s          => SerializeString( s ),
			//			char[] chars      => SerializeString( new string( chars ) ),
			IDictionary dict  => SerializeDictionary( dict ),
			IEnumerable items => SerializeArray( items ), // Array / Enumberable (must be after IDictionary)
			bool b            => SerializeBoolean( b ),
			int or uint or short or ushort or byte or sbyte	or long or ulong or float or double or decimal // 8 integers, 2 floating point, 1 decimal
				              => SerializeNumber( obj ),
			char k            => SerializeString( new string( k, 1 ) ),
			Enum              => SerializeString( obj.ToString() ),
			_                 => throw new Exception( "Cannot serialize object of type " + obj.GetType().Name )
		};
	}

	public string SerializeArray( IEnumerable enumerable ) {
		if(enumerable == null) { return "null"; }
		List<string> parts = new List<string>();
		_tabIndex++;
		foreach(object obj in enumerable)
			parts.Add( this.Serialize( obj ) );
		_tabIndex--;
		// string breaker = this.GetLineBreak();
		return this.ObjectJoin( "[]", parts );
	}

	public string SerializeDictionary( IDictionary dict ) {
		if(dict == null) { return "null"; }
		List<string> parts = new System.Collections.Generic.List<string>();
		_tabIndex++;
		foreach(DictionaryEntry entry in dict) {
			if(entry.Key is not string key)
				throw new ArgumentException( "Dictionary keys must be strings but key: " + entry.Key.ToString() + " of type:" + entry.Key.GetType().ToString() + " was found." );
			parts.Add( SerializeString( key ) + ":" + this.Serialize( entry.Value ) );
		}
		_tabIndex--;
		// string breaker = this.GetLineBreak();
		return this.ObjectJoin( "{}", parts );
	}

	#endregion public Dynamic Types

	#region public Simple-Value serializers (static)

	static public string SerializeNumber( object number ) => number.ToString();

	static public string SerializeBoolean( bool b ) => b ? "true" : "false";

	static public string SerializeString( string s ) {

		if(s == null) return "null";

		StringBuilder sb = new StringBuilder( s.Length + 4 );

		sb.Append( '"' );
		for(int i = 0; i < s.Length; i++) {
			char c = s[i];
			switch(c) {
				case '"': sb.Append( @"\""" ); break;
				case '\\': sb.Append( @"\\" ); break;
				case '/': sb.Append( @"\/" ); break;
				case '\b': sb.Append( @"\b" ); break;
				case '\f': sb.Append( @"\f" ); break;
				case '\n': sb.Append( @"\n" ); break;
				case '\r': sb.Append( @"\r" ); break;
				case '\t': sb.Append( @"\t" ); break;
				default:
					if(c < (char)32 || c > (char)127) {
						sb.Append( "\\u" );
						sb.Append( ((int)c).ToString( "x" ).PadLeft( 4, '0' ) );
					} else {
						sb.Append( c );
					}
					break;
			}

		}
		sb.Append( '"' );
		return sb.ToString();

	}

	#endregion

	#region private helper Serialize methods

	// constructs an object
	// builds arrays [] and ojbects {}
	string ObjectJoin( string openClose, List<string> parts ) {
		if(parts.Count == 0) { return openClose; } // no parts --> 1 line

		string breaker = this.GetLineBreak();
		StringBuilder builder = new StringBuilder();

		// open
		builder.Append( openClose[0] );

		foreach(string part in parts) {
			builder.Append( part );
			builder.Append( breaker );
			builder.Append( ',' );
		}
		builder.Length--;// remove last comma (this feels dirty but seems to be most efficient

		if(parts.Count == 1)
			builder.Length -= breaker.Length;

		// close
		builder.Append( openClose[1] );

		return builder.ToString();
	}

	// retuns the \n\t\t... combo form making json easier to read
	string GetLineBreak() {
		if(_useBreakers && _tabIndex <= this._tabMax) {
			StringBuilder builder = new StringBuilder( _tabIndex );
			builder.Append( '\r' );
			builder.Append( '\n' );
			builder.Append( '\t', _tabIndex );
			return builder.ToString();
		} else return string.Empty;
	}

	#endregion

	#region private fields
	int _tabIndex = 0;
	readonly int _tabMax;
	readonly bool _useBreakers;
	#endregion

}

/// <summary>
/// LL(1), recursive-descent, predictive parser for serializing/deserializing:
///        objects,enumerables,nulls,numbers, etc to JavaScript Object Notation
/// Use through JsonSerializer.
/// </summary>
internal class JsonDeserializer {

	#region constructor
	public JsonDeserializer( string json ) {
		_json = json;
	}
	#endregion
	
	#region public Dynamic Types

	public dynamic Deserialize() {
		EatWhiteSpace();
		char next = SafePeekChar();
		return next switch {
			'n'         => DeserializeNull(),
			'['         => DeserializeArray(),
			'{'         => DeserializeDictionary(),
			't' or 'f'  => DeserializeBoolean( false ).Value,
			'\'' or '"' => DeserializeString(),
			'0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '-' or '.'
			            => DeserializeNumber( false ).Value,
			_           => throw new FormatException( "Deserialize() encountered unexpected character: " + next ),
		};
	}

	public JsonArray DeserializeArray() {
		EatWhiteSpace();
		if(ReadNullIfThere()) return null;
		JsonArray jsonArray = new JsonArray();

		ReadSpecificChar( '[' );
		EatWhiteSpace();
		if(SafePeekChar() != ']')
			do {
				jsonArray.Add( this.Deserialize() );
			} while(TryReadChar( ',' ));
		ReadSpecificChar( ']' );
		return jsonArray;
	}

	public JsonObject DeserializeDictionary() {
		EatWhiteSpace();
		if(ReadNullIfThere()) return null;

		JsonObject jsonObject = new JsonObject();

		ReadSpecificChar( '{' );
		EatWhiteSpace();
		if(SafePeekChar() != '}') {
			do {
				string key = this.DeserializeString();
				ReadSpecificChar( ':' );
				var val = this.Deserialize();
				jsonObject.Add( key, val );

			} while(TryReadChar( ',' ));
		}
		ReadSpecificChar( '}' );
		return jsonObject;
	}

	#endregion public Dynamic Types

	#region public Simple-Value Types

	public double? DeserializeNumber( bool allowNulls ) {
		EatWhiteSpace();
		if(ReadNullIfThere())
			return allowNulls ? null : throw new FormatException( "Found null instead of number." );

		int start = _index;

		if(SafePeekChar() == '-') ++_index; // negative?
		while(char.IsDigit( SafePeekChar() )) ++_index; // whole #
		if(SafePeekChar() == '.') { // has decimal
			++_index;
			while(char.IsDigit( SafePeekChar() )) ++_index; // get decimal part
		}

		if(SafePeekChar() == 'E') {
			++_index;
			if(SafePeekChar() == '-') ++_index;
			int startOfE = _index;
			while(char.IsDigit( SafePeekChar() )) ++_index;
			if(_index == startOfE) throw new FormatException( "Number in scientific form but missing exponent." );
		}

		return double.Parse( _json[start.._index] );
	}

	public string DeserializeString() {
		EatWhiteSpace();
		if(ReadNullIfThere()) return null;

		char startChar = SafePeekChar();
		if(!IsQuote( startChar )) throw new FormatException( "Did not find starting quote of string." );
		UnsafeReadChar();

		StringBuilder buf = new StringBuilder();
		char k;
		while((k = ReadAnyCharOrThrowExceptionIfAtEnd()) != startChar) {
			// handle normal case
			if(k != '\\') {
				buf.Append( k );
				continue;
			}
			// escape characters
			k = ReadAnyCharOrThrowExceptionIfAtEnd();
			buf.Append( k switch {
				'b' => '\b',
				'f' => '\f',
				'n' => '\n',
				'r' => '\r',
				't' => '\t',
				'u' => (char)int.Parse( ReadString_Unknown( 4 ), System.Globalization.NumberStyles.HexNumber ),
				_ => k,
			} );

		}
		return buf.ToString();
	}

	public bool? DeserializeBoolean( bool allowNulls ) {
		EatWhiteSpace();
		if(allowNulls && ReadNullIfThere()) return null;
		switch(SafePeekChar()) {
			case 't': ReadString_Known( "true" ); return true;
			case 'f': ReadString_Known( "false" ); return false;
			default: throw new FormatException( "invalid boolean value" );
		}
	}

	#endregion

	#region private Helper methods

	object DeserializeNull() {
		ReadNullIfThere();
		return null;
	}

	static bool IsQuote( char k ) => k == '\'' || k == '"';

	char SafePeekChar() => AtEnd ? '\0' : _json[_index]; // if we are at the end, '\0' is returned
	char UnsafeReadChar() => _json[_index++]; // the index is not checked here, caller must check index before calling this.
	bool AtEnd => _index == _json.Length;

	int ReadIntoBuffer( char[] buf, int start, int length ) {
		int index = start;
		while(index < length && _index < _json.Length)
			buf[index++] = _json[_index++];
		return index - start;
	}

	void EatWhiteSpace() {
		while(0 <= "\r\n\t ".IndexOf( SafePeekChar() ))
			UnsafeReadChar();
		if(AtEnd)
			throw new FormatException( "Deserialize reached end of text reader without encountering any JSON objects." );
	}

	void ReadString_Known( string str ) {
		char[] buf = new char[str.Length];
		ReadIntoBuffer( buf, 0, str.Length );
		if(str != new string( buf ))
			throw new FormatException( "didn't read expected string: " + str );
	}

	string ReadString_Unknown( int length ) {
		char[] buf = new char[length];
		int bytesRead = ReadIntoBuffer( buf, 0, length );
		return bytesRead == length ? new string( buf )
			: throw new FormatException( string.Format( "Only read {0} when expecting {1}.", bytesRead, length ) );
	}

	// checks if there is a null and reads it in.
	bool ReadNullIfThere() {
		if(SafePeekChar() != 'n') return false;
		ReadString_Known( "null" );
		return true;
	}

	char ReadAnyCharOrThrowExceptionIfAtEnd() {
		return AtEnd ? throw new FormatException( "reached end of stream/string unexpectedly." ) 
			: UnsafeReadChar();
	}

	void ReadSpecificChar( char k ) {
		EatWhiteSpace();
		char readChar = ReadAnyCharOrThrowExceptionIfAtEnd();
		if(k != readChar)
			throw new FormatException( $"Expected {k} but found {readChar}" );
	}

	bool TryReadChar( char k ) {
		EatWhiteSpace();
		if(SafePeekChar() != k) return false;
		UnsafeReadChar();
		return true;
	}

	readonly string _json;
	int _index = 0;

	#endregion

}

/// <summary> Array of dynamic vaules. </summary>
public class JsonArray : List<dynamic> { };

/// <summary> Dictionary of string / dynamic pairs. </summary>
public class JsonObject : Dictionary<string, dynamic> { };