import OpenAI from "openai";
import fs from "fs";
import dotenv from "dotenv";
import { zodTextFormat } from "openai/helpers/zod";
import { InsuranceClaimSchema } from "./schema.ts";

dotenv.config();

const client = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

const systemPrompt =
  "You are an assistant whose job it is to extract structured data from text";

const text = await fs.promises.readFile("email-thread.md", {
  encoding: "utf-8",
});

const response = await client.responses.parse({
  model: "gpt-4o",
  instructions: systemPrompt,
  input: text,
  text: { format: zodTextFormat(InsuranceClaimSchema, "insurance_claim") },
});

if (response.output_parsed) {
  console.log("Extracted Claim Data:");
  console.log(JSON.stringify(response.output_parsed, null, 2));
} else {
  console.error("No parsed output received");
}
