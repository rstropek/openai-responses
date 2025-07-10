import { z } from 'zod';
import { FREQUENT_WORDS } from './frequent-words.ts';

export const BuildPasswordParameterListSchema = {
  minimumPasswordLength: z.number().int(),
};

export const BuildPasswordParametersSchema = z.object(BuildPasswordParameterListSchema);

export type BuildPasswordParameters = z.infer<typeof BuildPasswordParametersSchema>;

export const PasswordSchema = z.object({
  password: z.string(),
});

export type Password = z.infer<typeof PasswordSchema>;

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
