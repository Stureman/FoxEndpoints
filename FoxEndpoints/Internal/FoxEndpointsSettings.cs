using FoxEndpoints.Models;
using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints.Internal;

internal class FoxEndpointsSettings
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