using System.Text.Json;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace sisapi.infrastructure.Services.Reports;

public static class FunctionHelpers
{

    public static string SerializeDictionaryToQueryString(IDictionary<string, object>? dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return string.Empty;
        }

        var keyValuePairs = new List<string>();

        foreach (var kvp in dictionary)
        {
            var key = HttpUtility.UrlEncode(kvp.Key);
            var value = HttpUtility.UrlEncode(Convert.ToString(kvp.Value));
            keyValuePairs.Add($"{key}={value}");
        }

        return string.Join("&", keyValuePairs);
    }

    public static ReportParameters GetParametersForXml(IDictionary<string, object>? dictionary)
    {
        var parameters = new ReportParameters();

        if (dictionary == null || dictionary.Count == 0)
        {
            return parameters;
        }

        foreach (var kvp in dictionary)
        {
            var reportParameter = new ReportParameter
            {
                Name = kvp.Key,
                Value = FormatParameterValue(kvp.Value)
            };
            parameters.ReportParameter.Add(reportParameter);
        }

        return parameters;
    }
    
    private static string FormatParameterValue(object? value)
    {
        if (value == null) return string.Empty;

        return value switch
        {
            bool b => b ? "true" : "false",
            JsonElement jsonElement => FormatJsonElement(jsonElement),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }
    
    public static string SerializeXml<T>(T obj)
    {
        var emptyNamespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
        var serializer = new XmlSerializer(typeof(T));

        using var stream = new StringWriter();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });
        serializer.Serialize(writer, obj, emptyNamespaces);
        return stream.ToString();
    }
}


[XmlRoot(ElementName = "reportParameter")]
public class ReportParameter
{
    [XmlElement(ElementName = "value")]
    public string Value { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; } = string.Empty;
}

[XmlRoot(ElementName = "parameters")]
public class ReportParameters
{
    [XmlElement(ElementName = "reportParameter")]
    public List<ReportParameter> ReportParameter { get; set; } = [];
}

[XmlRoot(ElementName = "reportExecutionRequest")]
public class ReportExecutionRequest
{
    [XmlElement(ElementName = "reportUnitUri")]
    public string ReportUnitUri { get; set; } = string.Empty;

    [XmlElement(ElementName = "async")]
    public bool Async { get; set; }

    [XmlElement(ElementName = "freshData")]
    public bool FreshData { get; set; }

    [XmlElement(ElementName = "saveDataSnapshot")]
    public bool SaveDataSnapshot { get; set; }

    [XmlElement(ElementName = "outputFormat")]
    public string OutputFormat { get; set; } = "pdf";

    [XmlElement(ElementName = "interactive")]
    public bool Interactive { get; set; } = true;

    [XmlElement(ElementName = "ignorePagination")]
    public bool IgnorePagination { get; set; }

    [XmlElement(ElementName = "parameters", IsNullable = false)]
    public ReportParameters? Parameters { get; set; }

    public bool ShouldSerializeParameters()
    {
        return Parameters != null && Parameters.ReportParameter != null && Parameters.ReportParameter.Count > 0;
    }
}


public class Export
{
    public string status { get; set; } = string.Empty;
    public OutputResource? outputResource { get; set; }
    public string id { get; set; } = string.Empty;
}

public class OutputResource
{
    public string contentType { get; set; } = string.Empty;
    public string fileName { get; set; } = string.Empty;
    public bool outputFinal { get; set; }
    public int outputTimestamp { get; set; }
}

public class JasperResponse
{
    public string status { get; set; } = string.Empty;
    public int totalPages { get; set; }
    public string requestId { get; set; } = string.Empty;
    public string reportURI { get; set; } = string.Empty;
    public List<Export>? exports { get; set; }
}