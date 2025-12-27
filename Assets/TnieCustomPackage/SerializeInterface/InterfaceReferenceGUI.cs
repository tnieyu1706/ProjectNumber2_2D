using UnityEngine;

namespace TnieCustomPackage.SerializeInterface
{
    /// <summary>
	/// Serializes a UnityEngine.Object with the given interface. Adds a nice decorator in the inspector as well and a custom object selector.
	/// </summary>
	/// <typeparam name="TInterface">The interface.</typeparam>
	/// <typeparam name="UObject">The UnityEngine.Object.</typeparam>
	[System.Serializable]
	public class InterfaceReferenceGUI<TInterface, UObject> where UObject : Object where TInterface : class
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
				var @interface = _underlyingValue as TInterface;
				Debug.Assert(@interface != null, $"{_underlyingValue} needs to implement interface {nameof(TInterface)}.");
				return @interface;
			}
			set
			{
				if (value == null)
					_underlyingValue = null;
				else
				{
					var newValue = value as UObject;
					UnityEngine.Debug.Assert(newValue != null, $"{value} needs to be of type {typeof(UObject)}.");
					_underlyingValue = newValue;
				}
			}
		}
		/// <summary>
		/// Get the actual UnityEngine.Object that gets serialized.
		/// </summary>
		public UObject UnderlyingValue
		{
			get => _underlyingValue;
			set => _underlyingValue = value;
		}

		public InterfaceReferenceGUI() { }
		public InterfaceReferenceGUI(UObject target) => _underlyingValue = target;
		public InterfaceReferenceGUI(TInterface @interface) => _underlyingValue = @interface as UObject;

		public static implicit operator TInterface(InterfaceReferenceGUI<TInterface, UObject> obj) => obj.Value;
	}

	/// <summary>
	/// Serializes a UnityEngine.Object with the given interface. Adds a nice decorator in the inspector as well and a custom object selector.
	/// </summary>
	/// <typeparam name="TInterface">The interface.</typeparam>
	[System.Serializable]
	public class InterfaceReferenceGUI<TInterface> : InterfaceReferenceGUI<TInterface, Object> where TInterface : class
	{
	}
}