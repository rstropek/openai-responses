import OpenAI from "openai";
import type { EasyInputMessage } from "openai/resources/responses/responses";
import fs from "fs";
import { readLine } from "./input-helper.ts";
import dotenv from "dotenv";

dotenv.config();

const client = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

const systemPrompt = await fs.promises.readFile("system-prompt.md", {
  encoding: "utf-8",
});

const mode = process.argv[2] ?? "previous-id";

if (mode === "previous-id") {
  await runWithPreviousResponseId();
} else if (mode === "conversation") {
  await runWithConversationHistory();
} else {
  console.error(`Unknown mode: ${mode}. Use "previous-id" or "conversation".`);
  process.exit(1);
}

// Approach 1: Server-side state via previous_response_id
// The API stores the conversation; we just chain response IDs.
async function runWithPreviousResponseId() {
  console.log("Mode: previous_response_id (server-side state)\n");
  console.log("Bot: How can I help?\n");

  const userMessage = await readLine("You (empty to quit): ");
  if (!userMessage) process.exit(0);

  let response = await client.responses.create({
    model: "gpt-4o",
    instructions: systemPrompt,
    input: [{ role: "user", content: userMessage }],
    store: true,
  });
  let previousResponseId = response.id;

  while (true) {
    console.log(`\nBot: ${response.output_text}\n`);

    const userMessage = await readLine("You (empty to quit): ");
    if (!userMessage) break;

    response = await client.responses.create({
      model: "gpt-4o",
      previous_response_id: previousResponseId,
      instructions: systemPrompt,
      input: [{ role: "user", content: userMessage }],
      store: true,
    });

    previousResponseId = response.id;
  }
}

// Approach 2: Client-side state via manual conversation history
// We maintain the full message array ourselves.
async function runWithConversationHistory() {
  console.log("Mode: conversation history (client-side state)\n");
  console.log("Bot: How can I help?\n");

  const conversation: EasyInputMessage[] = [];

  const userMessage = await readLine("You (empty to quit): ");
  if (!userMessage) process.exit(0);

  conversation.push({ role: "user", content: userMessage });

  let response = await client.responses.create({
    model: "gpt-4o",
    instructions: systemPrompt,
    input: conversation,
  });

  while (true) {
    const assistantText = response.output_text;
    conversation.push({ role: "assistant", content: assistantText });

    console.log(`\nBot: ${assistantText}\n`);

    const userMessage = await readLine("You (empty to quit): ");
    if (!userMessage) break;

    conversation.push({ role: "user", content: userMessage });

    response = await client.responses.create({
      model: "gpt-4o",
      instructions: systemPrompt,
      input: conversation,
    });
  }
}
