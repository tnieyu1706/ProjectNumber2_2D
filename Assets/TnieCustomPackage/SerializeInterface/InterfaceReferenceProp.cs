using UnityEngine;

namespace TnieCustomPackage.SerializeInterface
{
	/// <summary>
	/// Serializes a UnityEngine.Object with the given interface. Adds a nice decorator in the inspector as well and a custom object selector.
	/// </summary>
	/// <typeparam name="TInterface">The interface.</typeparam>
	/// <typeparam name="UObject">The UnityEngine.Object.</typeparam>
	[System.Serializable]
	public class InterfaceReferenceProp<TInterface, UObject> where UObject : Object where TInterface : class
	{
		[SerializeField]
		[HideInInspector]
		private UObject _underlyingValue;

		/// <summary>
		/// Get the interface, if the UnderlyingValue is not null and implements the given interface.
		/// </summary>
		public TInterface Value
		{
			get
			{
				if (_underlyingValue == null)
					return null;

				switch (_underlyingValue)
				{
					case TInterface i:
						return i;

					case GameObject go:
						return go.GetComponent<TInterface>();

					case Component c:
						return c.GetComponent<TInterface>();

					default:
						return null;
				}
			}
			set
			{
				if (value == null)
				{
					_underlyingValue = null;
					return;
				}

				if (value is UObject uObj)
					_underlyingValue = uObj;
				else if (value is Component comp)
					_underlyingValue = comp as UObject;
				else
					Debug.LogError($"{value} not valid, cannot attach for InterfaceReference<{typeof(TInterface).Name},{typeof(UObject).Name}>");
			}
		}

		/// <summary>
		/// Get the actual UnityEngine.Object that gets serialized.
		/// </summary>
		public UObject UnderlyingValue
		{
			get => _underlyingValue;
			set
			{
				if (value is GameObject go)
				{
					// Nếu người dùng kéo thả GameObject, tự tìm component implement interface
					var component = go.GetComponent(typeof(TInterface)) as UObject;
					_underlyingValue = component;
				}
				else if (value is UObject uObj)
				{
					// Nếu là ScriptableObject hoặc Component trực tiếp
					if (value is TInterface)
						_underlyingValue = uObj;
					else
					{
						Debug.LogWarning($"{value.name} does not implement interface {typeof(TInterface).Name}");
						_underlyingValue = null;
					}
				}
				else
				{
					_underlyingValue = null;
				}
			}
		}

		public InterfaceReferenceProp() { }
		public InterfaceReferenceProp(UObject target) => _underlyingValue = target;
		public InterfaceReferenceProp(TInterface @interface) => _underlyingValue = @interface as UObject;

		public static implicit operator TInterface(InterfaceReferenceProp<TInterface, UObject> obj) => obj.Value;
	}

	/// <summary>
	/// Serializes a UnityEngine.Object with the given interface. Adds a nice decorator in the inspector as well and a custom object selector.
	/// </summary>
	/// <typeparam name="TInterface">The interface.</typeparam>
	[System.Serializable]
	public class InterfaceReferenceProp<TInterface> : InterfaceReferenceProp<TInterface, Object> where TInterface : class
	{
	}
}