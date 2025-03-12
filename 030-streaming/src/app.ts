import OpenAI from 'openai';
import fs from 'fs';
import { readLine } from './input-helper.ts';
import dotenv from 'dotenv';

dotenv.config();

const client = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

const systemPrompt = await fs.promises.readFile('system-prompt.md', {
  encoding: 'utf-8',
});

console.log("🤖: How can I help?");

let previousResponseId: string | undefined = undefined;

while (true) {
  const userMessage = await readLine('\nYou (empty to quit): ');
  if (!userMessage) {
    process.exit(0);
  }
 
  previousResponseId = await createResponse(client, previousResponseId, userMessage);
}

async function createResponse(client: OpenAI, previousResponseId: string | undefined, userMessage: string) {
  let response = await client.responses.create({
    model: 'gpt-4o',
    input: [{ role: 'user', content: userMessage }],
    stream: true,
    store: true,
    previous_response_id: previousResponseId,
  });

  for await (const event of response) {
    if (event.type === 'response.created') {
      previousResponseId = event.response.id;
    }
    if (event.type === 'response.output_text.delta') {
      process.stdout.write(event.delta);
    }
  }

  return previousResponseId;
}
