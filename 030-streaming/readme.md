# Streaming Responses

This sample demonstrates how to use streaming with the OpenAI Responses API to display assistant responses in real time as they are generated, rather than waiting for the complete response.

You will learn how to:

* Enable streaming by setting `stream: true` on the API call
* Process server-sent events as they arrive using an async iterator
* Handle specific event types like `response.created` and `response.output_text.delta`
* Combine streaming with `previous_response_id` for multi-turn conversations with real-time output
