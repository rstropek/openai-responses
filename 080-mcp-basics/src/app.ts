import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { buildPassword, BuildPasswordParameterListSchema, BuildPasswordParameters, BuildPasswordParametersSchema } from "./functions.ts";

const server = new McpServer({
  name: "mcp-password-generator",
  version: "1.0.0"
});

server.registerTool("build-password",
  {
    title: "Password Builder",
    description: "Generates an easy to remember password by concatinating random words from a list of frequently used words",
    inputSchema: BuildPasswordParameterListSchema
  },
  async (args: BuildPasswordParameters) => ({
    content: [{ type: "text", text: buildPassword(args) }]
  })
);

process.stderr.write("Starting MCP server...\n");

const transport = new StdioServerTransport();
await server.connect(transport);
