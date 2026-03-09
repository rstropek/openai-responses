# Storing Conversation State

This sample demonstrates two different approaches for maintaining conversation state across multiple turns with the OpenAI Responses API.

You will learn how to:

* Use `previous_response_id` to let OpenAI's server store the conversation history (server-side state)
* Manually manage a conversation history array on the client side (client-side state)
* Understand the trade-offs between both approaches
* Use the `store` parameter to enable server-side conversation storage
