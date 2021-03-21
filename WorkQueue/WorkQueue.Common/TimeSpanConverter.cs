using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkQueue.Common
{
	// https://github.com/dotnet/runtime/issues/29932
	internal class TimeSpanConverter : JsonConverter<TimeSpan>
	{
		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(format: null, CultureInfo.InvariantCulture));
		}
	}
}
