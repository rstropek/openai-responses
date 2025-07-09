import { AzureOpenAI} from 'openai';
import fs from 'fs';
import { readLine } from './input-helper.ts';
import { ResponseInput, ResponseInputItem } from 'openai/resources/responses/responses.mjs';
import dotenv from 'dotenv';

dotenv.config();

const client = new AzureOpenAI({
  apiKey: process.env.AZURE_OPENAI_API_KEY,
  endpoint: process.env.AZURE_ENDPOINT,
  deployment: process.env.AZURE_DEPLOYMENT,
  apiVersion: process.env.AZURE_API_VERSION,
});

const systemPrompt = await fs.promises.readFile('system-prompt.md', {
  encoding: 'utf-8',
});
const messages: ResponseInput = [
  {
    role: 'developer',
    content: systemPrompt,
  },
  {
    role: 'assistant',
    content: 'How can I help you?',
  },
];

while (true) {
  // print last message in messages
  const lastMessage = messages[messages.length - 1] as ResponseInputItem.Message;
  console.log(`\n🤖: ${lastMessage.content}`);

  // get user input
  const userMessage = await readLine('\nYou (empty to quit): ');
  if (!userMessage) {
    break;
  }

  // add user message to messages
  messages.push({
    role: 'user',
    content: userMessage,
  });

  const response = await client.responses.create({
    model: 'gpt-4o',
    input: messages,
  });

  messages.push({
    role: 'assistant',
    content: response.output_text,
  });
}
