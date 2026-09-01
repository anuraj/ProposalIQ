using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using System.Text;

namespace ProposalIQ.Web.Services;

public class ProposalTextExtractor : IProposalTextExtractor
{
    public async Task<string> ExtractTextAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file was uploaded.");
        }

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        await using var stream = file.OpenReadStream();

        return extension switch
        {
            ".txt" => await ReadTextAsync(
                stream,
                cancellationToken),

            ".docx" => ExtractDocx(stream),

            ".pdf" => ExtractPdf(stream),

            _ => throw new NotSupportedException(
                $"File type '{extension}' is not supported.")
        };
    }

    private static async Task<string> ReadTextAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string ExtractDocx(Stream stream)
    {
        using var document =
            WordprocessingDocument.Open(stream, false);

        var body = document.MainDocumentPart?
            .Document?
            .Body;

        if (body == null)
        {
            return string.Empty;
        }

        var paragraphs = body
            .Descendants<Paragraph>()
            .Select(p => p.InnerText)
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return string.Join(
            Environment.NewLine,
            paragraphs);
    }

    private static string ExtractPdf(Stream stream)
    {
        using var document = PdfDocument.Open(stream);

        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            builder.AppendLine(page.Text);
        }

        return builder.ToString();
    }
}