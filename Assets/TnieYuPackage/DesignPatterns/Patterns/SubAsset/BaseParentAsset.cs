using System.Collections.Generic;
using EditorAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Void = EditorAttributes.Void;

namespace TnieYuPackage.DesignPatterns.Patterns.SubAsset
{
    public abstract class BaseParentAsset : ScriptableObject
    {
        
    }
    
    public abstract class BaseParentAsset<TSub> : BaseParentAsset
        where TSub : BaseSubAsset
    {
        [SerializeField]
        [FoldoutGroup(
            "General SubAsset",
            nameof(generateFoldoutGroup),
            nameof(deletedFoldoutGroup)
            )]
        private Void generalSubAssetFoldout;
        
        public abstract List<TSub> SubAssets { get; }

        #region GENERATE_SUBASSET

        [SerializeField]
        [HideProperty]
        [FoldoutGroup(
            "Generate SubAsset",
            nameof(subAssetName),
            nameof(generateSubAssetButton)
        )]
        private Void generateFoldoutGroup;

        [SerializeField] [HideProperty] private string subAssetName;
        
        [SerializeField] [HideProperty, ButtonField(nameof(GenerateSubAsset), "Generate SubAsset")]
        private Void generateSubAssetButton;

        private void GenerateSubAsset()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(subAssetName))
            {
                Debug.Log("SubObjectName is null or empty");
                return;
            }

            var subAsset = ScriptableObject.CreateInstance<TSub>();
            subAsset.name = subAssetName;
            subAsset.Parent = this;
            HandleSubAssetWhenGenerating(subAsset);

            //add sub-asset
            AssetDatabase.AddObjectToAsset(subAsset, this);

            //add to list
            SubAssets.Add(subAsset);

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(subAsset);
            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }
        
        protected abstract void HandleSubAssetWhenGenerating(TSub subAsset);

        #endregion

        #region DELETE_SUBASSET

        [SerializeField]
        [HideProperty]
        [FoldoutGroup(
            "Delete SubAsset",
            nameof(deletingSubAsset),
            nameof(deleteSubAssetButton)
        )]
        private Void deletedFoldoutGroup;

        [SerializeField] [HideProperty] private TSub deletingSubAsset;

        [SerializeField] [HideProperty, ButtonField(nameof(DeleteSubAsset), "Delete SubAsset")]
        private Void deleteSubAssetButton;

        private void DeleteSubAsset()
        {
#if UNITY_EDITOR
            if (deletingSubAsset == null)
            {
                Debug.Log("Current Delete_Sub_Asset is null");
                return;
            }

            if (SubAssets.Contains(deletingSubAsset))
                SubAssets.Remove(deletingSubAsset);
            else
            {
                Debug.LogWarning($"Current Delete_Sub_Asset is not in children of this parent");
                return;
            }
            
            HandleSubAssetBeforeDelete(deletingSubAsset);
            Object.DestroyImmediate(deletingSubAsset, true);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

        protected abstract void HandleSubAssetBeforeDelete(TSub subAsset);

        #endregion
    }
}