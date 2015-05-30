using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace PocketArticle.Converters
{
    internal class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString() == "1";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }

    internal class PocketDateTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTimeString = reader.Value.ToString();
            DateTime dateTime;
            if (string.IsNullOrEmpty(dateTimeString) || !DateTime.TryParse(dateTimeString, out dateTime))
            {
                return null;
            }

            return dateTime;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }

    internal class ObjectToArrayConverter<T> : CustomCreationConverter<List<T>> where T : new()
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject;
            List<T> result = new List<T>();
            T target;

            // object is an array
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<List<T>>(reader);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            try
            {
                jObject = JObject.Load(reader);
            }
            catch
            {
                return null;
            }

            // Populate the object properties
            foreach (KeyValuePair<string, JToken> item in jObject)
            {
                target = new T();
                serializer.Populate(item.Value.CreateReader(), target);
                result.Add(target);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override List<T> Create(Type objectType)
        {
            return new List<T>();
        }
    }
}
