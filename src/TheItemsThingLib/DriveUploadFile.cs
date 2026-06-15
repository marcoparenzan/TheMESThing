using System;
using System.Collections.Generic;
using System.Text;

namespace TheItemsThingLib;

public sealed class DriveUploadFile
{
    public required string Name { get; init; }
    public required byte[] Content { get; init; }
    public string MimeType { get; init; } = "application/octet-stream";
}
