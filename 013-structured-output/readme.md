# Structured Output with Zod Schemas

This sample demonstrates how to extract structured data from unstructured text using OpenAI's Responses API with Zod schema validation. It parses an email thread about an insurance claim and returns a well-typed JSON object.

You will learn how to:

* Define a Zod schema describing the expected output structure
* Use `zodTextFormat` to instruct the API to return data matching the schema
* Use `responses.parse` to get a typed, validated result from the API
* Extract complex nested data (claimant info, incident details, timelines) from free-form text
