import OpenAI from 'openai';
import fs from 'fs';
import { readLine } from './input-helper.ts';
import { ResponseOutputText } from 'openai/resources/responses/responses.mjs';
import dotenv from 'dotenv';
import {
  getCustomerProductsRevenue,
  getCustomerProductsRevenueFunctionDefinition,
  getCustomers,
  getCustomersFunctionDefinition,
  getProducts,
  getProductsFunctionDefinition,
} from './functions.ts';
import { createConnectionPool } from './sql.ts';
import { ConnectionPool } from 'mssql';

dotenv.config();

const client = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });
const pool = await createConnectionPool(process.env.ADVENTURE_WORKS ?? '');

const systemPrompt = await fs.promises.readFile('system-prompt.md', {
  encoding: 'utf-8',
});

console.log('🤖: How can I help?');
let previousResponseId: string | undefined = undefined;

while (true) {
  const userMessage = await readLine('\nYou (empty to quit): ');
  if (!userMessage) {
    process.exit(0);
  }

  previousResponseId = await createResponse(client, pool, previousResponseId, userMessage);
}

async function createResponse(client: OpenAI, pool: ConnectionPool, previousResponseId: string | undefined, userMessage: string) {
  let input: any[] = [{ role: 'user', content: userMessage }];
  while (true) {
    let response = await client.responses.create({
      model: 'gpt-4.1',
      input,
      tool_choice: 'auto',
      tools: [getCustomersFunctionDefinition, getProductsFunctionDefinition, getCustomerProductsRevenueFunctionDefinition],
      store: true,
      previous_response_id: previousResponseId,
    });

    input = [];
    for (const event of response.output) {
      if (event.type === 'function_call') {
        let result: any;
        console.log(`${event.call_id}: Calling ${event.name} with arguments ${event.arguments}`);
        switch (event.name) {
          case getCustomersFunctionDefinition.name:
            result = await getCustomers(pool, JSON.parse(event.arguments));
            break;
          case getProductsFunctionDefinition.name:
            result = await getProducts(pool, JSON.parse(event.arguments));
            break;
          case getCustomerProductsRevenueFunctionDefinition.name:
            result = await getCustomerProductsRevenue(pool, JSON.parse(event.arguments));
            break;
        }

        input.push({
          type: 'function_call_output',
          call_id: event.call_id,
          output: JSON.stringify(result),
        });

        previousResponseId = response.id;
      } else if (event.type === 'message') {
        console.log((event.content[0] as ResponseOutputText).text);
        return response.id;
      }
    }
  }
}
