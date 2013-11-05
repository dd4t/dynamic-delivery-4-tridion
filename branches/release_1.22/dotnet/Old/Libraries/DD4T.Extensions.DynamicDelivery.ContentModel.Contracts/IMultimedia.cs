using System;
namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    public interface IMultimedia
    {
        string AltText { get; }
        string FileExtension { get; }
        string FileName { get; }
        int Height { get; }
        string MimeType { get; }
        int Size { get; }
        string Url { get; }
        int Width { get; }
    }
}
