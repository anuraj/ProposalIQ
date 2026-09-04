using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using ProposalIQ.Web.Services;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ProposalIQ.Web.Tests;

public class ProposalTextExtractorTests
{
    private readonly ProposalTextExtractor _extractor = new();

    [Fact]
    public async Task ExtractTextAsync_ReturnsSlideTextInPresentationOrder_ForPptx()
    {
        var file = CreateFormFile(
            "proposal.pptx",
            CreatePowerPoint(
                "First slide: implementation scope",
                "Second slide: pricing risk"));

        var text = await _extractor.ExtractTextAsync(file);

        Assert.Contains("First slide: implementation scope", text);
        Assert.Contains("Second slide: pricing risk", text);
        Assert.True(
            text.IndexOf("First slide", StringComparison.Ordinal) <
            text.IndexOf("Second slide", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExtractTextAsync_ReturnsEmptyString_WhenPptxHasNoReadableSlideText()
    {
        var file = CreateFormFile(
            "proposal.pptx",
            CreatePowerPoint(string.Empty));

        var text = await _extractor.ExtractTextAsync(file);

        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public async Task ExtractTextAsync_RejectsUnsupportedExtension_WithSupportedFormatsInMessage()
    {
        var file = CreateFormFile(
            "proposal.xlsx",
            "spreadsheet content"u8.ToArray());

        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _extractor.ExtractTextAsync(file));

        Assert.Contains(".xlsx", exception.Message);
        Assert.Contains(".txt", exception.Message);
        Assert.Contains(".docx", exception.Message);
        Assert.Contains(".pdf", exception.Message);
        Assert.Contains(".pptx", exception.Message);
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "proposalFile", fileName);
    }

    private static byte[] CreatePowerPoint(params string[] slideTexts)
    {
        using var stream = new MemoryStream();

        using (var document = PresentationDocument.Create(
            stream,
            PresentationDocumentType.Presentation))
        {
            var presentationPart = document.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation
            {
                SlideIdList = new P.SlideIdList()
            };

            var slideId = 256U;

            foreach (var slideText in slideTexts)
            {
                var slidePart = presentationPart.AddNewPart<SlidePart>();
                slidePart.Slide = CreateSlide(slideText);
                slidePart.Slide.Save();

                presentationPart.Presentation.SlideIdList.Append(new P.SlideId
                {
                    Id = slideId++,
                    RelationshipId = presentationPart.GetIdOfPart(slidePart)
                });
            }

            presentationPart.Presentation.Save();
        }

        return stream.ToArray();
    }

    private static P.Slide CreateSlide(string text)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties
                        {
                            Id = 1U,
                            Name = string.Empty
                        },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()),
                    new P.Shape(
                        new P.NonVisualShapeProperties(
                            new P.NonVisualDrawingProperties
                            {
                                Id = 2U,
                                Name = "Content"
                            },
                            new P.NonVisualShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()),
                        new P.ShapeProperties(),
                        new P.TextBody(
                            new A.BodyProperties(),
                            new A.ListStyle(),
                            new A.Paragraph(
                                new A.Run(
                                    new A.Text(text))))))));
    }
}