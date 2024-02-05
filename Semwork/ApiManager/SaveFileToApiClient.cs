using System.Net.Http.Headers;
using System.Text.Json;
using Semwork.Models;

namespace Semwork.ApiManager;

public partial class ApiClient
{
    public static async Task<string> SaveFile(string data, string type)
    {
        using (var httpClient = new HttpClient())
        {
            
            const string url = "https://api.bytescale.com/v2/accounts/12a1ymi/uploads/binary";
            const string authorizationHeader = "secret_12a1ymiDFnFwD47TXBENJEyrtkpv";
            
            var imageBytes = Convert.FromBase64String(data);
            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(type);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                authorizationHeader);
            

            var response = await httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode) return "";
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ResponseObject>(jsonResponse) ?? new ResponseObject();
            var link = responseObject.fileUrl;
            return link;
        }
    }
}

public class ResponseObject
{
    public string accountId { get; set; }
    public string filePath { get; set; }
    public string fileUrl { get; set; }
}