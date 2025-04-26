namespace MomentKeep.Core.Interfaces.External
{
    public interface IHuggingFaceClient
    {
        Task<byte[]> GenerateImageAsync(string prompt, string? style = null, string size = "512x512");
    }
}