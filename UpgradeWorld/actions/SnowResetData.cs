using System;
using System.IO;

namespace UpgradeWorld;

internal static class SnowResetData
{
  private struct Paint
  {
    internal bool Modified;
    internal float R, G, B, A;
  }

  // The callback returns generated snow at (x, y, terrain width), or null outside Deep North.
  internal static byte[]? Refill(byte[] data, Func<int, int, int, float?> naturalSnow, out int changed)
  {
    changed = 0;
    using var input = new MemoryStream(data, false);
    using var reader = new BinaryReader(input);
    if (reader.ReadInt32() != 1) throw new InvalidDataException("Unsupported terrain data version.");
    reader.ReadInt32(); // Operation count.
    for (var i = 0; i < 4; ++i) reader.ReadSingle(); // Last operation position and radius.
    var heightCount = reader.ReadInt32();
    var pitch = (int)Math.Sqrt(heightCount);
    if (heightCount < 4 || heightCount > data.Length || pitch * pitch != heightCount)
      throw new InvalidDataException("Invalid terrain height grid.");
    var width = pitch - 1;
    var heights = new bool[heightCount];
    for (var i = 0; i < heightCount; ++i)
    {
      heights[i] = reader.ReadBoolean();
      if (!heights[i]) continue;
      reader.ReadSingle();
      reader.ReadSingle();
    }
    var paintOffset = (int)input.Position;
    var paintCount = reader.ReadInt32();
    var legacy = paintCount == width * width;
    if (paintCount != heightCount && !legacy)
      throw new InvalidDataException("Invalid terrain paint grid.");
    var paintPitch = legacy ? width : pitch;
    var source = new Paint[paintCount];
    for (var i = 0; i < paintCount; ++i)
    {
      if (!reader.ReadBoolean()) continue;
      source[i] = new Paint { Modified = true, R = reader.ReadSingle(), G = reader.ReadSingle(), B = reader.ReadSingle(), A = reader.ReadSingle() };
    }
    if (input.Position != input.Length) throw new InvalidDataException("Unexpected trailing terrain data.");
    var paints = legacy ? new Paint[heightCount] : source;
    for (var y = 0; y < pitch; ++y)
    {
      for (var x = 0; x < pitch; ++x)
      {
        var index = y * pitch + x;
        // Match TerrainComp.Load's expansion of old width-by-width paint grids.
        var paint = source[Math.Min(y, paintPitch - 1) * paintPitch + Math.Min(x, paintPitch - 1)];
        paints[index] = paint;
        if (!paint.Modified || paint.R != 0f || paint.B != 0f || paint.G >= 1f || float.IsNaN(paint.G)) continue;
        // A paint sample lies half a cell southwest of its height vertex.
        if (HeightModified(x, y) || HeightModified(x - 1, y) || HeightModified(x, y - 1) || HeightModified(x - 1, y - 1)) continue;
        var target = naturalSnow(x, y, width);
        if (!target.HasValue || float.IsNaN(target.Value) || float.IsInfinity(target.Value)) continue;
        var snow = Math.Max(0f, Math.Min(1f, target.Value));
        if (!(paint.G < snow)) continue;
        paints[index].G = snow;
        changed++;
      }
    }
    if (changed == 0) return null;
    using var output = new MemoryStream();
    using var writer = new BinaryWriter(output);
    // Keep the header and every saved height value byte-for-byte unchanged.
    writer.Write(data, 0, paintOffset);
    writer.Write(heightCount);
    foreach (var paint in paints)
    {
      writer.Write(paint.Modified);
      if (!paint.Modified) continue;
      writer.Write(paint.R);
      writer.Write(paint.G);
      writer.Write(paint.B);
      writer.Write(paint.A);
    }
    return output.ToArray();

    bool HeightModified(int x, int y) => x >= 0 && y >= 0 && x < pitch && y < pitch && heights[y * pitch + x];
  }
}
