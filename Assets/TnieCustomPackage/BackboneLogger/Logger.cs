using System.Collections.Generic;
using UnityEngine;

namespace TnieCustomPackage.BackboneLogger
{
	/// <summary>
	/// Defines which parts of the log line will be colored.
	/// </summary>
	public enum LoggerColorMode
	{
		/// <summary>Do not use any color at all.</summary>
		NoColor,
		/// <summary>Only the category will be colored.</summary>
		CategoryOnly,

		/// <summary>The header and the message text will both be colored.</summary>
		FullMessage
	}

	/// <summary>
	/// Severity levels for logging messages.
	/// </summary>
	public enum LogLevel
	{
		/// <summary>Used for detailed debugging information.</summary>
		Debug,

		/// <summary>General informational messages.</summary>
		Info,

		/// <summary>Indicates a potential problem or unexpected situation.</summary>
		Warning,

		/// <summary>Indicates a recoverable error or serious issue.</summary>
		Error,

		/// <summary>Indicates a critical error that may cause the application to fail.</summary>
		Critical,

		/// <summary>No logs will be displayed at all.</summary>
		None
	}

	/// <summary>
	/// Central logging system for Unity with dynamic categories, severity levels, and per-category colors.
	/// </summary>
	public static class Logger
	{
		private static Dictionary<string, (bool active, Color color)> Categories => LoggerConfig.Instance.GetCategoriesDict();
		private static LoggerConfig LogConfig => LoggerConfig.Instance; 

		/// <summary>
		/// Enables or disables a specific category at runtime.
		/// </summary>
		/// <param name="category">The category name.</param>
		/// <param name="isActive">True to enable, false to disable.</param>
		public static void SetCategoryActive(string category, bool isActive)
		{
			if (!Categories.ContainsKey(category)) return;
			var current = Categories[category];
			Categories[category] = (isActive, current.color);
		}

		/// <summary>
		/// Checks whether a category is active.
		/// </summary>
		/// <param name="category">The category name.</param>
		/// <returns>True if the category is active, false otherwise.</returns>
		public static bool IsCategoryActive(string category)
		{
			return Categories.ContainsKey(category) && Categories[category].active;
		}

		/// <summary>
		/// Logs a message with the specified severity and category.
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <param name="level">The severity level.</param>
		/// <param name="category">The category name.</param>
		/// 
		public static void Log(string message, LogLevel level = LogLevel.Info, string category = "General")
		{
#if ENABLE_LOGGING
			// Filter out logs below the global level
			if (level < LogConfig.globalLevel) return;

			// Ignore logs from categories that are missing or inactive
			if (!Categories.ContainsKey(category) || !Categories[category].active) return;

			// Get the color configured for this category
			Color categoryColor = Categories[category].color;
			string categoryColorHex = ColorUtility.ToHtmlStringRGB(categoryColor);

			string fullMessage = "";

			switch (LogConfig.colorMode)
			{
				case LoggerColorMode.NoColor:
					fullMessage= $"[{level}] [{category}] {message}";
					break;
				case LoggerColorMode.CategoryOnly:
					fullMessage= $"[{level}] <color=#{categoryColorHex}>[{category}]</color> {message}";
					break;
				case LoggerColorMode.FullMessage:
					fullMessage = $"<color=#{categoryColorHex}>[{level}] [{category}] {message}</color>";
					break;
			}

			// Send to Unity's console with the appropriate method
			switch (level)
			{
				case LogLevel.Warning:
					Debug.LogWarning(fullMessage);
					break;

				case LogLevel.Error:
				case LogLevel.Critical:
					Debug.LogError(fullMessage);
					break;

				default:
					Debug.Log(fullMessage);
					break;
			}
#endif
		}
	}
}