namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IFileDownloadService
    {
        byte[] GetFileContents(string filePath);
    }
}
