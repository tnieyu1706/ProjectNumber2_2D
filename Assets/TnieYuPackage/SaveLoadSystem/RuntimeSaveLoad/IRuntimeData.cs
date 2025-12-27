using TnieYuPackage.Utils;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    public interface IRuntimeData
    {
        public SerializableGuid RuntimeId { get; set; }

        /// <summary>
        /// return json data string structure. record current runtimeData object.
        /// </summary>
        /// <returns></returns>
        string Save();

        /// <summary>
        /// transmitted inside json data to loading for runtimeData object.
        /// </summary>
        /// <param name="jsonData"></param>
        void Load(string jsonData);
    }
}