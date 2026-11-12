using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder( args );
// stdout carries the MCP protocol - all logging must go to stderr
builder.Logging.AddConsole( o => o.LogToStandardErrorThreshold = LogLevel.Trace );
builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithTools<SpiritIsland.Mcp.GameTools>();
await builder.Build().RunAsync();
