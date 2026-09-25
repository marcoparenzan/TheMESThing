using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheMESThing.Contracts.Json;

/// <summary>
/// Every quantity Ontly generates carries its canonical unit's symbol as a const
/// (CanonicalUnitSymbol, emitted by CSharpGenerator). This factory writes a quantity as a compact
/// two-element array - [341.65, "K"] - instead of the heavier {"value":341.65,"um":"K"}, and on
/// read requires the tagged unit to match the canonical one exactly. That last part is a
/// deliberate, narrow choice: accepting any of the quantity's other units on the wire (not just
/// the canonical one) would need full unit-name resolution against the generated {Name}Units
/// class, which this factory does not attempt - see PENDING.md.
/// </summary>
public sealed class QuantityJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType || !typeToConvert.IsGenericType) return false;
        var valueProperty = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        var symbolField = typeToConvert.GetField("CanonicalUnitSymbol", BindingFlags.Public | BindingFlags.Static);
        return valueProperty is { CanRead: true } && symbolField is not null
            && FindConstructor(typeToConvert, valueProperty.PropertyType) is not null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueProperty = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)!;
        var symbolField = typeToConvert.GetField("CanonicalUnitSymbol", BindingFlags.Public | BindingFlags.Static)!;
        var constructor = FindConstructor(typeToConvert, valueProperty.PropertyType)!;
        var converterType = typeof(QuantityJsonConverter<,>).MakeGenericType(typeToConvert, valueProperty.PropertyType);
        return (JsonConverter)Activator.CreateInstance(converterType, valueProperty, symbolField, constructor)!;
    }

    private static ConstructorInfo? FindConstructor(Type type, Type valueType) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(constructor => constructor.GetParameters() is [var parameter] && parameter.ParameterType == valueType);

    private sealed class QuantityJsonConverter<TQuantity, TValue>(PropertyInfo valueProperty, FieldInfo symbolField, ConstructorInfo constructor) : JsonConverter<TQuantity>
    {
        public override TQuantity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException($"Expected a [value, unit] array for {typeToConvert.Name}.");
            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            reader.Read();
            var symbol = reader.GetString();
            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException($"Expected exactly two elements in the [value, unit] array for {typeToConvert.Name}.");

            var canonicalSymbol = (string)symbolField.GetValue(null)!;
            if (!string.Equals(symbol, canonicalSymbol, StringComparison.Ordinal))
                throw new JsonException($"{typeToConvert.Name} only accepts its canonical unit '{canonicalSymbol}' on the wire; got '{symbol}'.");

            try
            {
                return (TQuantity)constructor.Invoke([value]);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                throw new JsonException(exception.InnerException.Message, exception.InnerException);
            }
        }

        public override void Write(Utf8JsonWriter writer, TQuantity value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            JsonSerializer.Serialize(writer, (TValue)valueProperty.GetValue(value)!, options);
            writer.WriteStringValue((string)symbolField.GetValue(null)!);
            writer.WriteEndArray();
        }
    }
}
