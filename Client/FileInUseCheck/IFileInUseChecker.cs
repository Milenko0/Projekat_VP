namespace Client.FileInUseCheck
{
    public interface IFileInUseChecker
    {
        bool IsFileInUse(string filePath);
    }
}
