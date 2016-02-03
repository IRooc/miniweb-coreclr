using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SampleWeb
{
	public class FileLogger : ILogger
	{
		private readonly object _lock = new object();
		private const int _indentation = 2;
		private readonly string _name;
		private Func<string, LogLevel, bool> _filter;
		private string _filename;

		public FileLogger(string name, Func<string, LogLevel, bool> filter, string filename)
		{
			_name = name;
			_filter = filter ?? ((category, logLevel) => true);
			_filename = filename;
		}

		public IDisposable BeginScopeImpl(object state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _filter(_name, logLevel);
		}

		private void FormatLogValues(StringBuilder builder, IReadOnlyList<KeyValuePair<string, object>> logValues, int level, bool bullet)
		{
			var values = logValues;
			if (values == null)
			{
				return;
			}
			var isFirst = true;
			foreach (var kvp in values)
			{
				builder.AppendLine();
				if (bullet && isFirst)
				{
					builder.Append(' ', level * _indentation - 1).Append('-');
				}
				else
				{
					builder.Append(' ', level * _indentation);
				}
				builder.Append(kvp.Key).Append(": ");

				if (kvp.Value is IEnumerable && !(kvp.Value is string))
				{
					foreach (var value in (IEnumerable)kvp.Value)
					{
						if (value is IReadOnlyList<KeyValuePair<string, object>>)
						{
							FormatLogValues(builder, (IReadOnlyList<KeyValuePair<string, object>>)value, level + 1, bullet: true);
						}
						else
						{
							builder.AppendLine()
									.Append(' ', (level + 1) * _indentation)
									.Append(value);
						}
					}
				}
				else if (kvp.Value is IReadOnlyList<KeyValuePair<string, object>>)
				{
					FormatLogValues(builder, (IReadOnlyList<KeyValuePair<string, object>>)kvp.Value, level + 1, bullet: false);
				}
				else
				{
					builder.Append(kvp.Value);
				}
				isFirst = false;
			}
		}
		

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}
			var message = string.Empty;
			var values = state as IReadOnlyList<KeyValuePair<string, object>>;
			if (formatter != null)
			{
				message = formatter(state, exception);
			}
			else if (values != null)
			{
				var builder = new StringBuilder();
				FormatLogValues(builder, values, level: 1, bullet: false);
				message = builder.ToString();
				if (exception != null)
				{
					message += Environment.NewLine + exception;
				}
			}
			else
			{
				message = Microsoft.Framework.Logging.LogFormatter.Formatter(state, exception);
			}
			if (string.IsNullOrEmpty(message))
			{
				return;
			}
			lock (_lock)
			{
				System.IO.File.AppendAllText(_filename, message + "\n");
			}
		}
	}
	public class FileLoggerProvider : ILoggerProvider
	{
		private readonly Func<string, LogLevel, bool> _filter;
		public string _fileName;

		public FileLoggerProvider(Func<string, LogLevel, bool> filter, string fileName)
		{
			_filter = filter;
			_fileName = fileName;
		}

		public ILogger CreateLogger(string name)
		{
			return new FileLogger(name, _filter, _fileName);
		}

		public void Dispose()
		{

		}
	}
}

