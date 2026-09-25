using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheMESThing.Contracts.Json;

/// <summary>
/// Every scalar Ontly generates has the identical shape: a non-generic value type with exactly
/// one public "Value" property and a matching single-argument constructor. That uniformity - the
/// whole point of generating them instead of hand-writing each one - is what lets a single
/// reflection-based factory serialize all of them as their bare value ("CELL-01") instead of a
/// wrapper object ({"value":"CELL-01"}), without a converter per type and without touching the
/// generated code itself. Quantities (TemperatureMeasure&lt;TNumber&gt; and friends) share the
/// same shape but are generic - deliberately excluded here, since flattening one to a bare number
/// would silently drop which unit it's canonical in.
/// </summary>
public sealed class ScalarJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType || typeToConvert.IsGenericType) return false;
        var valueProperty = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        return valueProperty is { CanRead: true } && FindConstructor(typeToConvert, valueProperty.PropertyType) is not null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueProperty = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)!;
        var constructor = FindConstructor(typeToConvert, valueProperty.PropertyType)!;
        var converterType = typeof(ScalarJsonConverter<,>).MakeGenericType(typeToConvert, valueProperty.PropertyType);
        return (JsonConverter)Activator.CreateInstance(converterType, valueProperty, constructor)!;
    }

    private static ConstructorInfo? FindConstructor(Type type, Type valueType) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(constructor => constructor.GetParameters() is [var parameter] && parameter.ParameterType == valueType);

    private sealed class ScalarJsonConverter<TScalar, TValue>(PropertyInfo valueProperty, ConstructorInfo constructor) : JsonConverter<TScalar>
    {
        public override TScalar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            try
            {
                return (TScalar)constructor.Invoke([value]);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                // The domain constructor rejected the value (e.g. ArgumentOutOfRangeException).
                // Reflection wraps that in TargetInvocationException; unwrap it into a JsonException
                // so ASP.NET Core's own body-binding error handling turns it into a clean 400,
                // the same as any other malformed request body.
                throw new JsonException(exception.InnerException.Message, exception.InnerException);
            }
        }

        public override void Write(Utf8JsonWriter writer, TScalar value, JsonSerializerOptions options)
        {
            var inner = (TValue)valueProperty.GetValue(value)!;
            JsonSerializer.Serialize(writer, inner, options);
        }
    }
}
