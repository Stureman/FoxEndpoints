using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints.Internal;

/// <summary>
/// Stores runtime configuration shared across FoxEndpoints components.
/// </summary>
internal static class FoxEndpointsSettings
{
    private static FormOptions _formOptions = CreateDefaultFormOptions();
    private static FileBindingMode _fileBindingMode = FileBindingMode.Buffered;

    internal static FormOptions FormOptions => _formOptions;
    internal static FileBindingMode FileBindingMode => _fileBindingMode;

    internal static void ConfigureFormOptions(FormOptions options)
    {
        _formOptions = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal static void SetFileBindingMode(FileBindingMode mode)
    {
        _fileBindingMode = mode;
    }

    private static FormOptions CreateDefaultFormOptions()
        => new()
        {
            BufferBody = true,
            MemoryBufferThreshold = 64 * 1024,
            MultipartBodyLengthLimit = 20 * 1024 * 1024 // 20 MB guard rail
        };
}