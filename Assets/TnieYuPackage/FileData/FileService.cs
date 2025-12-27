using System;
using System.Collections.Generic;

namespace TnieYuPackage.FileData
{
    public interface IFileService
    {
        string Path { get; }
        
        void WriteData<T>(IEnumerable<T> data);
        IEnumerable<T> ReadData<T>();
    }
    
    [Serializable]
    public abstract class FileService<TService> : IFileService
        where TService : new()
    {
        public abstract string Path { get; }

        public TService service = new TService();

        public abstract void WriteData<T>(IEnumerable<T> data);
        public abstract IEnumerable<T> ReadData<T>();
    }
}