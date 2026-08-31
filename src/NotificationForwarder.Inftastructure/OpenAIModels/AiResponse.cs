using System.Text.Json.Serialization;

namespace NotificationForwarder.Infrastructure.OpenAIModels;

internal sealed record ApiResponse(
    [property:JsonPropertyName("output")] IReadOnlyList<OutputItem>? Output)
{
    public string? GetOutputText() => Output?
        .FirstOrDefault(item => item.Type == "message")?
        .Content?
        .FirstOrDefault(item => item.Type == "output_text")?
        .Text;
}

internal sealed record OutputItem(
    [property: JsonPropertyName("type")] string? Type, 
    [property: JsonPropertyName("content")] IReadOnlyList<ContentItem>? Content);

internal sealed record ContentItem(
    [property: JsonPropertyName("type")] string? Type, 
    [property: JsonPropertyName("text")] string? Text);
