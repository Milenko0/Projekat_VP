namespace Client.FileSending
{
    public interface IFileSender
    {
        void SendFiles();
        void SendFile(string filePath);
    }
}
