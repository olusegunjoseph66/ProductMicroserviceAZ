﻿using System.Globalization;
using System.Text.Json;

namespace Product.API.Middlewares
{
    public class DecimalConverter : System.Text.Json.Serialization.JsonConverter<decimal>
    {
        public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        {
            string stringValue = reader.GetString();
            return string.IsNullOrWhiteSpace(stringValue)
                ? default
                : decimal.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        public override void Write(
            Utf8JsonWriter writer,
            decimal value,
            JsonSerializerOptions options)
        {
            string numberAsString = value.ToString("F2", CultureInfo.InvariantCulture);
            writer.WriteStringValue(numberAsString);
        }
    }
}
