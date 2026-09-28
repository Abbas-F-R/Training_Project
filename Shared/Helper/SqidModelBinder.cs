using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Training_Project.Shared.Attributes;
using Sqids;

namespace Training_Project.Shared.Helper;

/// <summary>
/// Model binder for decoding Sqid strings from route values and query parameters into numeric IDs.
/// </summary>
public class SqidModelBinder : IModelBinder
{
    private static readonly SqidsEncoder<long> Encoder = new(new SqidsOptions { MinLength = 8 });

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        if (long.TryParse(value, out var longValue))
        {
            bindingContext.Result = ModelBindingResult.Success(longValue);
            return Task.CompletedTask;
        }

        try
        {
            var result = Encoder.Decode(value);
            if (result.Count > 0)
            {
                bindingContext.Result = ModelBindingResult.Success(result[0]);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid Sqid format.");
            }
        }
        catch
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Failed to decode Sqid.");
        }

        return Task.CompletedTask;
    }
}

public class SqidModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var isLong = context.Metadata.ModelType == typeof(long) || context.Metadata.ModelType == typeof(long?);
        if (!isLong) return null;

        var hasSqidAttr = false;
        if (context.Metadata is DefaultModelMetadata defaultMetadata)
        {
            hasSqidAttr = defaultMetadata.Attributes.Attributes.OfType<SqidAttribute>().Any();
        }

        var isIdentifierName = context.Metadata.PropertyName != null &&
            (context.Metadata.PropertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
             context.Metadata.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase));

        if (hasSqidAttr || isIdentifierName)
        {
            return new SqidModelBinder();
        }

        return null;
    }
}
