using System.Net.Http.Json;
using System.Text.Json;

namespace UrlShort.Client;

class Program
{
    private const string ApiKey = "";
    private const string Username = "";
    private const string BaseUrl = "http://localhost:5268/api/url";

    static async Task Main(string[] args)
    {
        Console.WriteLine("URL Shortener REST API Client");
        Console.WriteLine($"User: {Username}");
        Console.WriteLine();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Username", Username);
        client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);

        try
        {
            var authCheck = await client.GetAsync(BaseUrl);
            if (!authCheck.IsSuccessStatusCode) {
                Console.WriteLine("Authentication failed. Please check your ApiKey and Username");
                return;
            }
        }
        catch { return; }

        var options = new JsonSerializerOptions { WriteIndented = true };

        while (true)
        {
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. List all your short URLs");
            Console.WriteLine("2. Create a new short URL");
            Console.WriteLine("3. Update an existing URL");
            Console.WriteLine("4. Delete an existing URL");
            Console.WriteLine("5. Create a new category");
            Console.WriteLine("6. Delete a category");
            Console.WriteLine("7. Exit");
            Console.Write("Choose an option: ");
            
            var choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await HandleGet(client, options);
                        break;
                    case "2":
                        await HandlePost(client);
                        break;
                    case "3":
                        await HandlePut(client);
                        break;
                    case "4":
                        await HandleDelete(client);
                        break;
                    case "5":
                        await HandleCreateCategory(client);
                        break;
                    case "6":
                        await HandleDeleteCategory(client);
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid option, try again");
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error. Target URL: {BaseUrl}");
                Console.WriteLine(ex.Message);
            }
            
            Console.WriteLine();
        }
    }

    private static async Task HandleGet(HttpClient client, JsonSerializerOptions options)
    {
        Console.WriteLine("Fetching URLs");
        var response = await client.GetAsync(BaseUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
            Console.WriteLine("Your URLs:");
            Console.WriteLine(JsonSerializer.Serialize(jsonElement, options));
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task HandlePost(HttpClient client)
    {
        Console.Write("Enter the long URL you want to shorten: ");
        var longUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(longUrl))
        {
            Console.WriteLine("URL cannot be empty");
            return;
        }

        var catResponse = await client.GetAsync($"{BaseUrl}/categories");
        if (catResponse.IsSuccessStatusCode)
        {
            var catContent = await catResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(catContent);
            var categoriesArray = jsonDoc.RootElement.EnumerateArray().ToList();
            
            if (categoriesArray.Any())
            {
                Console.WriteLine("Available Categories:");
                foreach (var cat in categoriesArray)
                {
                    Console.WriteLine($"  [{cat.GetProperty("id")}] - {cat.GetProperty("name")}");
                }
            }
            else
            {
                Console.WriteLine("You have no categories.");
            }
        }

        Console.Write("Enter Category ID (leave empty to skip): ");
        var categoryInput = Console.ReadLine();
        int? categoryId = null;
        if (int.TryParse(categoryInput, out int parsedCatId))
        {
            categoryId = parsedCatId;
        }

        Console.Write("Enter custom short code (leave empty for random): ");
        var customCode = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(customCode)) customCode = null;

        var payload = new { OriginalUrl = longUrl, CategoryId = categoryId, CustomCode = customCode };
        var response = await client.PostAsJsonAsync(BaseUrl, payload);
        
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(contentString);
            var shortCode = jsonDoc.RootElement.GetProperty("shortCode").GetString();
            
            Console.WriteLine("Successfully created new short URL");
            Console.WriteLine($"Your short link: http://localhost:5268/r/{shortCode}");
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task HandlePut(HttpClient client)
    {
        Console.Write("Enter the ID of the URL you want to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID");
            return;
        }

        Console.Write("Enter the NEW long URL: ");
        var newLongUrl = Console.ReadLine();


        var payload = new { OriginalUrl = newLongUrl, IsActive = true };
        var response = await client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"URL with ID {id} has been successfully updated");
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task HandleDelete(HttpClient client)
    {
        Console.Write("Enter the ID of the URL you want to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var response = await client.DeleteAsync($"{BaseUrl}/{id}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"URL with ID {id} has been successfully deleted");
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task HandleCreateCategory(HttpClient client)
    {
        Console.Write("Enter new category name: ");
        var name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Category name cannot be empty");
            return;
        }

        var payload = new { Name = name };
        var response = await client.PostAsJsonAsync($"{BaseUrl}/categories", payload);
        
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(contentString);
            var id = jsonDoc.RootElement.GetProperty("id").GetInt32();
            
            Console.WriteLine($"Successfully created new category '{name}' with ID: {id}");
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task HandleDeleteCategory(HttpClient client)
    {
        var catResponse = await client.GetAsync($"{BaseUrl}/categories");
        if (catResponse.IsSuccessStatusCode)
        {
            var catContent = await catResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(catContent);
            var categoriesArray = jsonDoc.RootElement.EnumerateArray().ToList();
            
            if (categoriesArray.Any())
            {
                Console.WriteLine("Available Categories:");
                foreach (var cat in categoriesArray)
                {
                    Console.WriteLine($"  [{cat.GetProperty("id")}] - {cat.GetProperty("name")}");
                }
            }
            else
            {
                Console.WriteLine("You have no categories to delete.");
                return;
            }
        }

        Console.Write("Enter the ID of the category you want to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var response = await client.DeleteAsync($"{BaseUrl}/categories/{id}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Category with ID {id} has been successfully deleted");
        }
        else
        {
            await PrintError(response);
        }
    }

    private static async Task PrintError(HttpResponseMessage response)
    {
        Console.WriteLine($"Action failed with status: {response.StatusCode}");
        var errorContent = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(errorContent))
        {
            Console.WriteLine($"Details: {errorContent}");
        }
    }
}
