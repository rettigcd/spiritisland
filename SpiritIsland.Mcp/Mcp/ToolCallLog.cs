using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace SpiritIsland.Mcp;

/// <summary>
/// Appends one CSV row per tool call (request size, response size, rough token estimate, time) so we can see
/// which tools burn the client's context. Sizes are in characters of text the model sees.
/// Path: env var SPIRITISLAND_TOOL_LOG, else tool-calls.csv next to the executable.
/// </summary>
public static class ToolCallLog {

	public static string FilePath { get; } = Environment.GetEnvironmentVariable( "SPIRITISLAND_TOOL_LOG" )
		is { Length: > 0 } p ? p : Path.Combine( AppContext.BaseDirectory, "tool-calls.csv" );

	public static McpRequestHandler<CallToolRequestParams, CallToolResult> Filter(
		McpRequestHandler<CallToolRequestParams, CallToolResult> next
	) => async ( context, cancellationToken ) => {
		var sw = Stopwatch.StartNew();
		string tool = context.Params?.Name ?? "?";
		int requestChars = Size( context.Params );
		try {
			CallToolResult result = await next( context, cancellationToken );
			Write( tool, requestChars, Size( result ), result.IsError == true, sw.ElapsedMilliseconds );
			return result;
		} catch( Exception ex ) {
			// the SDK turns McpException into an error result; ex.Message approximates what the client sees
			Write( tool, requestChars, ex is McpException ? ex.Message.Length : 0, true, sw.ElapsedMilliseconds );
			throw;
		}
	};

	/// <summary>Characters of argument JSON the model wrote.</summary>
	static int Size( CallToolRequestParams? p ) {
		try { return p?.Arguments is null ? 0 : JsonSerializer.Serialize( p.Arguments ).Length; }
		catch { return -1; } // never let logging break a tool call
	}

	/// <summary>Characters the model reads: the text content (unescaped), not the wire JSON, which escapes every quote as ".</summary>
	static int Size( CallToolResult r ) {
		try {
			int n = r.Content.Sum( c => c is TextContentBlock t ? t.Text.Length : JsonSerializer.Serialize( c, McpJsonUtilities.DefaultOptions ).Length );
			if( r.StructuredContent is { } s ) n += s.ToString().Length;
			return n;
		} catch { return -1; }
	}

	static void Write( string tool, int requestChars, int responseChars, bool isError, long ms ) {
		try {
			lock( _lock ) {
				bool isNew = !File.Exists( FilePath );
				using var w = new StreamWriter( FilePath, append: true );
				if( isNew )
					w.WriteLine( "timestamp,session,tool,requestChars,responseChars,approxTokens,isError,ms" );
				int approxTokens = (requestChars + responseChars) / 4; // ~4 chars per token for JSON
				w.WriteLine( string.Join( ",",
					DateTime.Now.ToString( "s", CultureInfo.InvariantCulture ), _session, tool,
					requestChars, responseChars, approxTokens, isError ? 1 : 0, ms ) );
			}
		} catch( IOException ) { /* logging is best-effort */ }
	}

	static readonly object _lock = new();
	static readonly string _session = DateTime.Now.ToString( "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture );
}
