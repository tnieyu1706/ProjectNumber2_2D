using System;
using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.FileData
{
    [Serializable]
    public class CsvService : FileService<CsvSerializer>
    {
        [SerializeField] [TniePath(typeof(TextAsset), ".csv")]
        private string filePath = string.Empty;
        
        public override string Path => filePath;

        public override void WriteData<T>(IEnumerable<T> datas)
        {
            if (Path == string.Empty)
            {
                Debug.LogWarning("Path is empty.");
                return;
            }
            
            service.WriteListData(Path, datas);
            Debug.Log($"Data written to {Path}");
        }

        public override IEnumerable<T> ReadData<T>()
        {
            if (Path == string.Empty)
            {
                Debug.LogWarning("Path is empty.");
                return null;
            }
            
            var datas = service.ReadListData<T>(filePath);
            Debug.Log($"Data read from {Path}");
            return datas;
        }
    }
}