# Embeddings-Based RAG (Retrieval-Augmented Generation)

This sample demonstrates a complete RAG pipeline using OpenAI embeddings and a SQL Server product database. It finds relevant products by comparing embedding similarities and uses the results to augment the AI's response.

You will learn how to:

* Generate and cache embeddings for product descriptions from a database
* Compute cosine similarity (via dot product) between a user query and stored product embeddings
* Select the most relevant products based on embedding similarity
* Build an augmented prompt that includes the retrieved product data to ground the AI's response
* Combine embeddings-based retrieval with the Responses API for a full RAG workflow
