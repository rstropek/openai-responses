import OpenAI from "openai";
import fs from "fs";
import dotenv from "dotenv";
import { z } from "zod/v4";
import { InsuranceClaimSchema } from "./schema.ts";

dotenv.config();

const client = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

const systemPrompt =
  "You are an assistant whose job it is to extract structured data from text";

const text = await fs.promises.readFile("email-thread.md", {
  encoding: "utf-8",
});

const response = await client.responses.create({
  model: "gpt-4o",
  instructions: systemPrompt,
  input: text,
  text: {
    format: {
      type: "json_schema",
      name: "insurance_claim",
      strict: true,
      schema: z.toJSONSchema(InsuranceClaimSchema),
    },
  },
});

if (response.status === "completed") {
  // Output extracted contract data or handle errors
  const res = response.output[0];
  if (res.type === "message") {
    const content = res.content[0];
    if (content.type === "refusal") {
      console.error(content.refusal);
    } else if (content.type === "output_text") {
      console.log(`Extracted Claim Data:\n${content.text}`);
    } else {
      throw new Error("No response content");
    }
  }
}
