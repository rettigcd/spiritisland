using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder( args );
// stdout carries the MCP protocol - all logging must go to stderr
builder.Logging.AddConsole( o => o.LogToStandardErrorThreshold = LogLevel.Trace );
builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithTools<SpiritIsland.Mcp.GameTools>()
	.WithRequestFilters( f => f.AddCallToolFilter( SpiritIsland.Mcp.ToolCallLog.Filter ) );
var app = builder.Build();
app.Services.GetRequiredService<ILogger<Program>>()
	.LogInformation( "Logging tool-call sizes to {Path}", SpiritIsland.Mcp.ToolCallLog.FilePath );
await app.RunAsync();
