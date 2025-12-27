using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TnieYuPackage.FileData
{
    [Serializable]
    public class JsonSerializer
    {
        public void WriteData<T>(string filepath, T data)
        {
            if (data == null)
            {
                Debug.LogWarning("Data is null!");
                return;
            }

            string jsonData = JsonUtility.ToJson(data, true);

            File.WriteAllText(filepath, jsonData, Encoding.UTF8);
        }

        public T ReadData<T>(string filepath)
        {
            string jsonData = File.ReadAllText(filepath, Encoding.UTF8);

            return JsonUtility.FromJson<T>(jsonData);
        }
    }
}