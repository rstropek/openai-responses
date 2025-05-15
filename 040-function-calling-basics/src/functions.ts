import sql from 'mssql';
import { FunctionTool } from 'openai/resources/responses/responses.mjs';
import { FREQUENT_WORDS } from './frequent-words.ts';

export const buildPasswordTool: FunctionTool = {
  type: 'function',
  name: 'buildPassword',
  description: 'Generates an easy to remember password by concatinating a random word from the list of frequent words with a random number.',
  parameters: {
    type: 'object',
    properties: {
      minimumPasswordLength: { type: 'integer', description: 'Minimum length of the password. Set to 0 for default length (15 characters)' },
    },
    required: ['minimumPasswordLength'],
    additionalProperties: false,
  },
  strict: true,
};

export type BuildPasswordParameters = {
  minimumPasswordLength: number;
};

export type Password = {
  password: string;
};

export function buildPassword(filter: BuildPasswordParameters): string {
  function getRandomWord(): string {
    return FREQUENT_WORDS[Math.floor(Math.random() * FREQUENT_WORDS.length)];
  }

  let password = '';
  while (password.length < filter.minimumPasswordLength) {
    const nextWord = getRandomWord();
    if (password.length > 0) {
      password += nextWord[0].toUpperCase() + nextWord.slice(1);
    } else {
      password += nextWord;
    }
  }

  return password;
}
