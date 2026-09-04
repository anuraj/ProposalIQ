using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;

namespace ProposalIQ.Web.Services;

public class ProposalTextExtractor : IProposalTextExtractor
{
    private static readonly string[] SupportedFileExtensions =
    {
        ".txt",
        ".docx",
        ".pdf",
        ".pptx"
    };

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

            ".pptx" => ExtractPptx(stream),

            _ => throw new NotSupportedException(
                $"File type '{extension}' is not supported. Supported file types are {string.Join(", ", SupportedFileExtensions)}.")
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

    private static string ExtractPptx(Stream stream)
    {
        using var document = PresentationDocument.Open(stream, false);

        var presentationPart = document.PresentationPart;
        if (presentationPart == null)
        {
            return string.Empty;
        }

        var slideIds = presentationPart
            .Presentation?
            .SlideIdList?
            .Elements<SlideId>();

        if (slideIds == null)
        {
            return string.Empty;
        }

        var slides = new List<string>();

        foreach (var slideId in slideIds)
        {
            var relationshipId = slideId.RelationshipId?.Value;

            if (string.IsNullOrWhiteSpace(relationshipId))
            {
                continue;
            }

            var slidePart = presentationPart.GetPartById(relationshipId) as SlidePart;
            var slideText = ExtractSlideText(slidePart);

            if (!string.IsNullOrWhiteSpace(slideText))
            {
                slides.Add(slideText);
            }
        }

        return string.Join(
            Environment.NewLine,
            slides);
    }

    private static string ExtractSlideText(SlidePart? slidePart)
    {
        if (slidePart?.Slide == null)
        {
            return string.Empty;
        }

        var paragraphs = slidePart.Slide
            .Descendants<A.Paragraph>()
            .Select(paragraph => string.Concat(
                paragraph
                    .Descendants<A.Text>()
                    .Select(text => text.Text ?? string.Empty)))
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