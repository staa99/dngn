using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DngnApiBackend.Utilities
{
    public class DictionaryTKeyEnumTValueConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   (typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                    typeToConvert.GetGenericTypeDefinition() == typeof(IDictionary<,>)) &&
                   typeToConvert.GetGenericArguments()[0].IsEnum;
        }


        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            var converter = (JsonConverter?) Activator.CreateInstance(
                typeof(DictionaryEnumConverterInner<,>).MakeGenericType(keyType, valueType),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[] {options},
                null);

            if (converter == null)
            {
                throw new ApplicationException("A data processing error occurred: NULL_CONVERTER");
            }

            return converter;
        }


        private class DictionaryEnumConverterInner<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
            where TKey : struct, Enum
        {
            private readonly Type _keyType;
            private readonly JsonConverter<TValue> _valueConverter;
            private readonly Type _valueType;


            public DictionaryEnumConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<TValue>) options
                    .GetConverter(typeof(TValue));

                // Cache the key and value types.
                _keyType   = typeof(TKey);
                _valueType = typeof(TValue);
            }


            public override IDictionary<TKey, TValue> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var dictionary = new Dictionary<TKey, TValue>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return dictionary;
                    }

                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string propertyName = reader.GetString();

                    // For performance, parse with ignoreCase:false first.
                    if (!Enum.TryParse(propertyName, false, out TKey key) &&
                        !Enum.TryParse(propertyName, true, out key))
                    {
                        throw new JsonException(
                            $"Unable to convert \"{propertyName}\" to Enum \"{_keyType}\".");
                    }

                    // Get the value.
                    TValue value;
                    if (_valueConverter != null)
                    {
                        reader.Read();
                        value = _valueConverter.Read(ref reader, _valueType, options);
                    }
                    else
                    {
                        value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    }

                    // Add to dictionary.
                    dictionary.Add(key, value);
                }

                throw new JsonException();
            }


            public override void Write(
                Utf8JsonWriter writer,
                IDictionary<TKey, TValue> dictionary,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (var (key, value) in dictionary)
                {
                    var propertyName = key.ToString();
                    writer.WritePropertyName
                        (options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

                    if (_valueConverter != null)
                    {
                        _valueConverter.Write(writer, value, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, value, options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}