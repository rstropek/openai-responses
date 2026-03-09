# Function Calling with Database Access

This sample extends the function calling pattern by connecting the AI assistant to a SQL Server database (AdventureWorks). The assistant can query customers, products, and revenue data to answer business questions.

You will learn how to:

* Define multiple function tools that query a SQL Server database
* Let the AI decide which functions to call based on the user's natural language question
* Handle parameterized SQL queries triggered by function calls
* Chain multiple function calls in a single conversation turn to gather the data needed for a complete answer
