# OpenAI **Response Playground**

A curated collection of minimal, _self-contained_ samples that demonstrate how to use the **[OpenAI](https://platform.openai.com/docs/)** SDKs (Node.js & .NET) to build conversational applications, stream completions, call functions, work with a database, and expose responses over **Server-Sent Events (SSE)**.

> **Goal** – provide ready-to-run reference projects you can use as a learning tool or a starting point for your own experiments.

---

## Repository layout

| Sample | Runtime | Highlights |
|--------|---------|------------|
| `010-basics` | Node 18+ | Smallest possible chat loop – reads a system prompt and keeps the conversation in memory. |
| `011-basics-dotnet` | .NET 8 | C# port of the basics sample. |
| `020-store-state` | Node 18+ | Persists the conversation on the OpenAI side by using the **store/previous_response_id** features. |
| `021-store-state-dotnet` | .NET 8 | C# version of the stateful chat. |
| `030-streaming` | Node 18+ | Streams assistant deltas to the console in real-time (`stream: true`). |
| `031-streaming-dotnet` | .NET 8 | C# streaming sample using async-enumerables. |
| `040-function-calling-basics` | Node 18+ | Demonstrates **function calling**: the assistant can invoke an in-process `buildPassword` function defined through JSON-schema. |
| `041-function-calling-basics-dotnet` | .NET 8 | C# version of the password builder function call. |
| `050-function-calling-db` | Node 18+ | Shows how to expose **SQL queries** as callable functions (get customers / products / revenue). Requires an Azure SQL connection string. |
| `051-function-calling-db-dotnet` | .NET 8 | C# equivalent of the DB-backed function calling sample. |
| `061-sse-dotnet` | .NET 8 & Vite | End-to-end chat delivered through **Server-Sent Events**: ASP.NET Core backend + vanilla JS frontend. |

---

## Prerequisites

1. **OpenAI account & API key** – create one at <https://platform.openai.com/>.  
   You will need to make it available to the samples via the `OPENAI_API_KEY` variable (see below).
2. **Node.js 18 LTS+** – required for the TypeScript samples.
3. **.NET 8 SDK** – required for the C# samples.
4. *(optional – only `050/051`)* **Azure SQL Database** with the AdventureWorksLT sample data if you want to run the database-centric projects.

---

## One-time setup

Clone the repo and move into it:

```bash
git clone https://github.com/<you>/openai-response-playground.git
cd openai-response-playground
```

### 1. Provide your OpenAI API key

Most projects look for the key in the environment variable `OPENAI_API_KEY`.

• **Node.js samples** – each folder contains a `dotenv.template`.

```bash
cp 010-basics/dotenv.template 010-basics/.env
# edit the new .env and paste your key
```

• **.NET samples** – export it in your shell _or_ append it to the user secrets/appsettings:

```bash
export OPENAI_API_KEY="sk-..."
```

### 2. Install dependencies (per sample)

Node sample:

```bash
cd 010-basics
npm install
```

.NET sample:

```bash
cd 011-basics-dotnet
# restores on first run automatically
dotnet build
```

You can of course repeat the steps for as many folders as you want to explore.

---

## Running a sample

```bash
# Node – interactive chat loop
dcd 030-streaming
npm start

# .NET – run via dotnet
dotnet run --project 031-streaming-dotnet
```

Some projects print **🤖 How can I help?** and wait for your input – type something and press **ENTER** to chat. Press **ENTER** on an empty line to exit.

### Database samples (`050` / `051`)

These need a SQL Server connection string (`SQL_CONNECTION_STRING`) in addition to the API key.  The template already contains the variable name – copy it like this:

```bash
cp 050-function-calling-db/dotenv.template 050-function-calling-db/.env
# fill out SQL_CONNECTION_STRING in .env
```

> Tip – you can use [Azure SQL AdventureWorksLT](https://learn.microsoft.com/azure-sql/) or any SQL Server that contains similar tables.

---

## How the samples work

Below is a very high-level view of the building blocks (check the source for full details):

1. **System prompt** – each folder has a `system-prompt.md` describing the assistant persona and obeyed rules.
2. **Conversation loop** – relies on a small helper (`input-helper.ts` / `ReadLineAsync` in C#) to read user input.
3. **OpenAI SDK** – `openai` NPM package or `OpenAI.Responses` NuGet package is configured with your API key and used to call `client.responses.create`.
4. **Streaming & SSE** – when `stream: true` is passed, the code asynchronously yields deltas which are displayed immediately (or forwarded via SSE in `061`).
5. **Function calling** – samples `040+` declare JSON tools, inspect `response.output` for `function_call` events, execute local code/SQL, then feed the results back.

---

## Extending / building your own

Feel free to copy a folder and tweak:

• change the model (`gpt-4o`, `gpt-4.1`…),  
• add more tools,  
• swap the database access layer,  
• replace the CLI with a web UI (see `061` for an example).

Pull requests that improve documentation or sample quality are welcome!

---

## License

MIT – see [`LICENSE`](LICENSE) for details.