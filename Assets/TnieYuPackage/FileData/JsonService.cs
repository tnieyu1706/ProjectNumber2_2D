using System;
using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.FileData
{
    [Serializable]
    public class JsonItemsWrapper<T>
    {
        public T[] items;
    }
    
    [Serializable]
    public class JsonService : FileService<JsonSerializer>
    {
        [SerializeField] [TniePath(typeof(TextAsset), ".json")] 
        private string filePath;
        
        public override string Path => filePath;
        
        public override void WriteData<T>(IEnumerable<T> data)
        {
            if (filePath == null)
            {
                Debug.LogError("file path is null");
                return;
            }
            
            var itemsWrapper = new JsonItemsWrapper<T>()
            {
                items = data.ToArray()
            };
            
            service.WriteData(filePath, itemsWrapper);
            Debug.Log($"Json data written from - {filePath}");
        }

        public override IEnumerable<T> ReadData<T>()
        {
            if (filePath == null)
            {
                Debug.LogError("file path is null");
                return null;
            }

            JsonItemsWrapper<T> itemsWrapper = service.ReadData<JsonItemsWrapper<T>>(filePath);
            
            Debug.Log($"Json data read from - {filePath}");
            
            return itemsWrapper.items;
        }

        public void WriteObject<T>(T data)
        {
            if (Path == string.Empty)
            {
                Debug.LogWarning("Path is empty.");
                return;
            }
            
            service.WriteData(Path, data);
            Debug.Log($"JsonData has been written to {filePath}");
        }

        public T ReadObject<T>()
        {
            if (Path == string.Empty)
            {
                Debug.LogWarning("Path is empty.");
                return default(T);
            }
            var data = service.ReadData<T>(Path);

            Debug.Log($"Have just read data from JsonData-{filePath}");
            return data;
        }
    }
}