using System.Numerics;
using Veldrid;

namespace Synesthesia.Engine.Graphics;

struct VertexPositionColor(Vector2 Position, RgbaFloat Color)
{
    public Vector2 Position = Position;
    public RgbaFloat Color = Color;

    public const uint SizeInBytes = 24;
}