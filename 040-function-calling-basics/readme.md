# Function Calling Basics

This sample demonstrates how to use function calling (tool use) with the OpenAI Responses API. The AI assistant can call a local password generator function when the user asks for a password.

You will learn how to:

* Define a function tool with a JSON schema describing its parameters
* Register tools when creating a response
* Detect function call requests in the API response and execute the corresponding local function
* Return function results back to the API to continue the conversation
* Implement a tool-use loop that handles multiple rounds of function calls
