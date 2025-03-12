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

const userMessage = await readLine('\nYou (empty to quit): ');
if (!userMessage) {
  process.exit(0);
}

let response = await client.responses.create({
  model: 'gpt-4o',
  instructions: systemPrompt,
  input: [{ role: 'user', content: userMessage }],
  store: true,
});
let previousResponseId = response.id;

while (true) {
  console.log(`\n🤖: ${response.output_text}`);

  const userMessage = await readLine('\nYou (empty to quit): ');
  if (!userMessage) {
    break;
  }

  response = await client.responses.create({
    model: 'gpt-4o',
    previous_response_id: previousResponseId,
    instructions: systemPrompt,
    input: [{ role: 'user', content: userMessage }],
    store: true,
  });
  
  previousResponseId = response.id;
}
