using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///
/// </summary>
/// <summary>
/// <para>
/// The converter we create needs to know the exact type we're converting, otherwise you would only get base object properties every
/// time. Therefore it has to be generic. So the Factory creates the right Converter instance type for the object and sends it on its' way.
/// </para>
/// <para>
/// For more details,
/// <see href="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-6-0">see Microsoft's converter documentation.</see>
/// </para>
/// </summary>
public class OptInJsonConverterFactory : JsonConverterFactory
{
	/// <summary>
	///
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <returns></returns>
	public override bool CanConvert(Type typeToConvert) => typeToConvert.GetCustomAttribute<JsonOptInAttribute>() != null;

	/// <summary>
	///
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(OptInJsonConverter<>).MakeGenericType(new Type[] { typeToConvert }),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: null,
			culture: null)!;

		return converter;
	}
}

/// <summary>
///
/// </summary>
public class OptInJsonConverter<T> : JsonConverter<T>
{
	// //RWM:
	// private static readonly List<string> _propertiesToIgnore = new()
	// {
	// 	nameof(IChangeTracking.IsChanged),
	// 	nameof(DbObservableObject.IsGraphChanged),
	// 	nameof(DbObservableObject.ShouldTrackChanges)
	// };

	/// <summary>
	///
	/// </summary>
	public override bool HandleNull => false;

	/// <summary>
	///
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert,
		JsonSerializerOptions options)
	{
		return JsonSerializer.Deserialize<T>(ref reader, options);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		if (value is not null)
		{
			writer.WriteStartObject();

			foreach (var property in value.GetType().GetProperties().Where(c => c.GetCustomAttribute<JsonIncludeAttribute>() != null))
			{
				var propValue = property.GetValue(value);
				switch (true)
				{
					case true when propValue is not null:
					case true when propValue is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.Never:
						writer.WritePropertyName(property.Name);
						JsonSerializer.Serialize(writer, propValue, options);
						break;
				}
			}

			writer.WriteEndObject();
		}
	}
}

public class JsonOptInAttribute : Attribute
{

}

public class ObjectHandleJsonConverterFactory : JsonConverterFactory
{
	/// <summary>
	///
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <returns></returns>
	public override bool CanConvert(Type typeToConvert) => typeToConvert.GetCustomAttribute<JsonOptInAttribute>() != null;

	/// <summary>
	///
	/// </summary>
	/// <param name="typeToConvert"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(ObjectHandleJsonConverter<>).MakeGenericType(new Type[] { typeToConvert }),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: null,
			culture: null)!;

		return converter;
	}
}

public class JsonObjectHandle
{
//	public string ObjType { get; set; }

	public object Obj { get; set; }
}

public class ObjectHandleJsonConverter<T> : JsonConverter<T>
{
	private static readonly JsonSerializerOptions _opt = new()
	{
		Converters = { new OptInJsonConverterFactory() },
	};

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.Read();
		var propName1 = reader.GetString();
		reader.Read();
		var propVal1 = reader.GetString();

		reader.Read();
		var propName2 = reader.GetString();

		reader.Read();

		var obj = JsonSerializer.Deserialize<T>(ref reader, _opt);

		return obj;
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		if (value is not null)
		{
			writer.WriteStartObject();

				writer.WriteString("obj_type", value.GetType().FullName);

				writer.WritePropertyName("obj");

				writer.WriteStartObject();

				foreach (var property in value.GetType().GetProperties().Where(c => c.GetCustomAttribute<JsonIncludeAttribute>() != null))
				{
					var propValue = property.GetValue(value);
					switch (true)
					{
						case true when propValue is not null:
						case true when propValue is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.Never:
							writer.WritePropertyName(property.Name);
							JsonSerializer.Serialize(writer, propValue, options);
							break;
					}
				}
				writer.WriteEndObject();

			writer.WriteEndObject();
		}
	}
}