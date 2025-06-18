using OpenAI.Responses;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FunctionCallingBasics;

// Parameter records for each function
public record GetCustomersParameters(
    int CustomerID,
    string FirstName,
    string MiddleName,
    string LastName,
    string CompanyName
);

public record GetProductsParameters(
    int ProductID,
    string Name,
    string ProductNumber
);

public record GetCustomerProductsRevenueParameters(
    int CustomerID,
    int ProductID,
    int Year,
    int Month,
    bool GroupByCustomer,
    bool GroupByProduct,
    bool GroupByYear,
    bool GroupByMonth
);

// Result records
public record Customer(
    int CustomerID,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? CompanyName
);

public record Product(
    int ProductID,
    string Name,
    string ProductNumber
);

public record CustomerProductsRevenue(
    decimal Revenue,
    int? CustomerID,
    int? ProductID,
    int? Year,
    int? Month
);

public static class DatabaseFunctions
{
    public static readonly ResponseTool GetCustomersTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetCustomers),
        functionDescription: "Gets a filtered list of customers. At least one filter MUST be provided in the parameters. The result list is limited to the first 25 customers.",
        functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "customerID": {
                    "type": "integer",
                    "description": "Optional filter for the customer ID. Use 0 for no filter."
                },
                "firstName": {
                    "type": "string",
                    "description": "Optional filter for the first name (true if first name contains filter value). Use empty string for no filter."
                },
                "middleName": {
                    "type": "string",
                    "description": "Optional filter for the middle name (true if middle name contains filter value). Use empty string for no filter."
                },
                "lastName": {
                    "type": "string",
                    "description": "Optional filter for the last name (true if last name contains filter value). Use empty string for no filter."
                },
                "companyName": {
                    "type": "string",
                    "description": "Optional filter for the company name (true if company name contains filter value). Use empty string for no filter."
                }
            },
            "required": ["customerID", "firstName", "middleName", "lastName", "companyName"],
            "additionalProperties": false
        }
        """u8.ToArray()),
        functionSchemaIsStrict: true
    );

    public static readonly ResponseTool GetProductsTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetProducts),
        functionDescription: "Gets a filtered list of products. At least one filter MUST be provided in the parameters. The result list is limited to 25 products.",
        functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "productID": {
                    "type": "integer",
                    "description": "Optional filter for the product ID. Use 0 for no filter."
                },
                "name": {
                    "type": "string",
                    "description": "Optional filter for the product name (true if product name contains filter value). Use empty string for no filter."
                },
                "productNumber": {
                    "type": "string",
                    "description": "Optional filter for the product number. Use empty string for no filter."
                }
            },
            "required": ["productID", "name", "productNumber"],
            "additionalProperties": false
        }
        """u8.ToArray()),
        functionSchemaIsStrict: true
    );

    public static readonly ResponseTool GetCustomerProductsRevenueTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetCustomerProductsRevenue),
        functionDescription: "Gets the revenue of the customer and products. The result is ordered by the revenue in descending order. The result list is limited to 25 records.",
        functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "customerID": {
                    "type": "integer",
                    "description": "Optional filter for the customer ID. Use 0 for no filter."
                },
                "productID": {
                    "type": "integer",
                    "description": "Optional filter for the product ID. Use 0 for no filter."
                },
                "year": {
                    "type": "integer",
                    "description": "Optional filter for the year. Use 0 for no filter."
                },
                "month": {
                    "type": "integer",
                    "description": "Optional filter for the month. Use 0 for no filter."
                },
                "groupByCustomer": {
                    "type": "boolean",
                    "description": "If true, revenue is grouped by customer ID."
                },
                "groupByProduct": {
                    "type": "boolean",
                    "description": "If true, revenue is grouped by product ID."
                },
                "groupByYear": {
                    "type": "boolean",
                    "description": "If true, revenue is grouped by year."
                },
                "groupByMonth": {
                    "type": "boolean",
                    "description": "If true, revenue is grouped by month."
                }
            },
            "required": ["customerID", "productID", "year", "month", "groupByCustomer", "groupByProduct", "groupByYear", "groupByMonth"],
            "additionalProperties": false
        }
        """u8.ToArray()),
        functionSchemaIsStrict: true
    );

    public static async Task<List<Customer>> GetCustomers(SqlConnection connection, GetCustomersParameters filter)
    {
        if (filter.CustomerID == 0 && string.IsNullOrEmpty(filter.FirstName) && 
            string.IsNullOrEmpty(filter.MiddleName) && string.IsNullOrEmpty(filter.LastName) && 
            string.IsNullOrEmpty(filter.CompanyName))
        {
            throw new ArgumentException("At least one filter must be provided.");
        }

        using var command = new SqlCommand();
        command.Connection = connection;
        
        var query = "SELECT TOP 25 CustomerID, FirstName, MiddleName, LastName, CompanyName FROM SalesLT.Customer WHERE CustomerID >= 29485";
        
        if (filter.CustomerID > 0)
        {
            query += " AND CustomerID = @customerID";
            command.Parameters.Add("@customerID", SqlDbType.Int).Value = filter.CustomerID;
        }
        
        if (!string.IsNullOrEmpty(filter.FirstName))
        {
            query += " AND FirstName LIKE '%' + @firstName + '%'";
            command.Parameters.Add("@firstName", SqlDbType.NVarChar).Value = filter.FirstName;
        }
        
        if (!string.IsNullOrEmpty(filter.MiddleName))
        {
            query += " AND MiddleName LIKE '%' + @middleName + '%'";
            command.Parameters.Add("@middleName", SqlDbType.NVarChar).Value = filter.MiddleName;
        }
        
        if (!string.IsNullOrEmpty(filter.LastName))
        {
            query += " AND LastName LIKE '%' + @lastName + '%'";
            command.Parameters.Add("@lastName", SqlDbType.NVarChar).Value = filter.LastName;
        }
        
        if (!string.IsNullOrEmpty(filter.CompanyName))
        {
            query += " AND CompanyName LIKE '%' + @companyName + '%'";
            command.Parameters.Add("@companyName", SqlDbType.NVarChar).Value = filter.CompanyName;
        }

        command.CommandText = query;
        Console.WriteLine($"Executing query: {query}");

        var customers = new List<Customer>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            customers.Add(new Customer(
                CustomerID: reader.GetInt32("CustomerID"),
                FirstName: reader.GetString("FirstName"),
                MiddleName: reader.IsDBNull("MiddleName") ? null : reader.GetString("MiddleName"),
                LastName: reader.GetString("LastName"),
                CompanyName: reader.IsDBNull("CompanyName") ? null : reader.GetString("CompanyName")
            ));
        }

        return customers;
    }

    public static async Task<List<Product>> GetProducts(SqlConnection connection, GetProductsParameters filter)
    {
        if (filter.ProductID == 0 && string.IsNullOrEmpty(filter.Name) && string.IsNullOrEmpty(filter.ProductNumber))
        {
            throw new ArgumentException("At least one filter must be provided.");
        }

        using var command = new SqlCommand();
        command.Connection = connection;
        
        var query = "SELECT TOP 25 ProductID, Name, ProductNumber, ProductCategoryID FROM SalesLT.Product WHERE 1 = 1";
        
        if (filter.ProductID > 0)
        {
            query += " AND ProductID = @productID";
            command.Parameters.Add("@productID", SqlDbType.Int).Value = filter.ProductID;
        }
        
        if (!string.IsNullOrEmpty(filter.Name))
        {
            query += " AND Name LIKE '%' + @name + '%'";
            command.Parameters.Add("@name", SqlDbType.NVarChar).Value = filter.Name;
        }
        
        if (!string.IsNullOrEmpty(filter.ProductNumber))
        {
            query += " AND ProductNumber = @productNumber";
            command.Parameters.Add("@productNumber", SqlDbType.NVarChar).Value = filter.ProductNumber;
        }

        command.CommandText = query;
        Console.WriteLine($"Executing query: {query}");

        var products = new List<Product>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            products.Add(new Product(
                ProductID: reader.GetInt32("ProductID"),
                Name: reader.GetString("Name"),
                ProductNumber: reader.GetString("ProductNumber")
            ));
        }

        return products;
    }

    public static async Task<List<CustomerProductsRevenue>> GetCustomerProductsRevenue(SqlConnection connection, GetCustomerProductsRevenueParameters filter)
    {
        using var command = new SqlCommand();
        command.Connection = connection;
        
        var query = "SELECT TOP 25 SUM(LineTotal) AS Revenue";
        
        if (filter.GroupByCustomer) query += ", CustomerID";
        if (filter.GroupByProduct) query += ", ProductID";
        if (filter.GroupByYear) query += ", YEAR(OrderDate) AS Year";
        if (filter.GroupByMonth) query += ", MONTH(OrderDate) AS Month";

        query += " FROM SalesLT.SalesOrderDetail d INNER JOIN SalesLT.SalesOrderHeader h ON d.SalesOrderID = h.SalesOrderID WHERE 1 = 1";

        if (filter.CustomerID > 0)
        {
            query += " AND CustomerID = @customerID";
            command.Parameters.Add("@customerID", SqlDbType.Int).Value = filter.CustomerID;
        }
        
        if (filter.ProductID > 0)
        {
            query += " AND ProductID = @productID";
            command.Parameters.Add("@productID", SqlDbType.Int).Value = filter.ProductID;
        }
        
        if (filter.Year > 0)
        {
            query += " AND YEAR(OrderDate) = @year";
            command.Parameters.Add("@year", SqlDbType.Int).Value = filter.Year;
        }
        
        if (filter.Month > 0)
        {
            query += " AND MONTH(OrderDate) = @month";
            command.Parameters.Add("@month", SqlDbType.Int).Value = filter.Month;
        }

        if (filter.GroupByCustomer || filter.GroupByProduct || filter.GroupByYear || filter.GroupByMonth)
        {
            var groupColumns = new List<string>();
            if (filter.GroupByCustomer) groupColumns.Add("CustomerID");
            if (filter.GroupByProduct) groupColumns.Add("ProductID");
            if (filter.GroupByYear) groupColumns.Add("YEAR(OrderDate)");
            if (filter.GroupByMonth) groupColumns.Add("MONTH(OrderDate)");
            query += $" GROUP BY {string.Join(", ", groupColumns)}";
        }

        query += " ORDER BY SUM(LineTotal) DESC";

        command.CommandText = query;
        Console.WriteLine($"Executing query: {query}");

        var results = new List<CustomerProductsRevenue>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            results.Add(new CustomerProductsRevenue(
                Revenue: reader.GetDecimal("Revenue"),
                CustomerID: filter.GroupByCustomer && !reader.IsDBNull("CustomerID") ? reader.GetInt32("CustomerID") : null,
                ProductID: filter.GroupByProduct && !reader.IsDBNull("ProductID") ? reader.GetInt32("ProductID") : null,
                Year: filter.GroupByYear && !reader.IsDBNull("Year") ? reader.GetInt32("Year") : null,
                Month: filter.GroupByMonth && !reader.IsDBNull("Month") ? reader.GetInt32("Month") : null
            ));
        }

        return results;
    }
} 