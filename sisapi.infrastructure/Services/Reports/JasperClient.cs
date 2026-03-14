using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;

namespace sisapi.infrastructure.Services.Reports;

public class JasperClient
{
    private readonly string _jasperServerUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly IHttpClientFactory? _httpClientFactory;

    public JasperClient(string jasperServerUrl, string username, string password, IHttpClientFactory? httpClientFactory = null)
    {
        _jasperServerUrl = jasperServerUrl ?? throw new ArgumentNullException(nameof(jasperServerUrl));
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _httpClientFactory = httpClientFactory;
    }

    public async Task<byte[]> GetReportAsync(string reportPath, IDictionary<string, object>? parameters,
        string format = "pdf", bool ignorePagination = false)
    {
        return await GetReportWithXmlAsync(reportPath, parameters, format, ignorePagination);
    }

    public async Task<byte[]> GetReportWithXmlAsync(string reportPath, IDictionary<string, object>? parameters,
        string format = "pdf", bool ignorePagination = false)
    {
        try
        {
            using var client = CreateHttpClient();

            var parametersObj = FunctionHelpers.GetParametersForXml(parameters);

            var xmlRequest = new ReportExecutionRequest
            {
                ReportUnitUri = reportPath,
                Parameters = (parametersObj?.ReportParameter?.Count > 0) ? parametersObj : null,
                IgnorePagination = ignorePagination,
                OutputFormat = format.ToLower()
            };

            string xml = FunctionHelpers.SerializeXml(xmlRequest);
            Console.WriteLine("=== XML Request ===");
            Console.WriteLine(xml);
            Console.WriteLine("===================");

            string endPointJr = $"{_jasperServerUrl}/rest_v2/reportExecutions";
            Console.WriteLine($"Endpoint: {endPointJr}");

            var content = new StringContent(xml, Encoding.UTF8, "application/xml");
            var response = await client.PostAsync(endPointJr, content);

            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseString}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Jasper Server returned status {response.StatusCode}: {responseString}");
            }

            var data = JsonSerializer.Deserialize<JasperResponse>(responseString);

            if (data?.status == "ready" && data.exports != null && data.exports.Count > 0 && data.exports[0].status == "ready")
            {
                if (data.totalPages > 0)
                {
                    string endPointDw = $"{endPointJr}/{data.requestId}/exports/{data.exports[0].id}/outputResource";
                    byte[] reportBytes = await client.GetByteArrayAsync(endPointDw);
                    return reportBytes;
                }
                else
                {
                    throw new Exception("No data available for the report");
                }
            }
            else
            {
                throw new Exception($"Error executing report: {responseString}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Jasper report with XML: {ex.Message}", ex);
        }
    }
    
    public async Task<string> GetReportBase64Async(string reportPath, IDictionary<string, object>? parameters,
        string format = "pdf", bool ignorePagination = false)
    {
        byte[] reportBytes = await GetReportWithXmlAsync(reportPath, parameters, format, ignorePagination);
        return Convert.ToBase64String(reportBytes);
    }

    private HttpClient CreateHttpClient()
    {
        HttpClient client;

        if (_httpClientFactory != null)
        {
            client = _httpClientFactory.CreateClient();
        }
        else
        {
            client = new HttpClient();
        }

        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return client;
    }
}