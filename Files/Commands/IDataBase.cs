using System.Collections.Generic;

namespace Files.Commaands
{
    public interface IDataBase
    {
        bool InsertFile(string fileName, byte[] fileData);
        Dictionary<string, byte[]> GetFileData(string fileBeginsWith);
    }
}
