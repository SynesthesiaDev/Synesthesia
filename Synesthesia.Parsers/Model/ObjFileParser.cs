// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Numerics;
using Synesthesia.Common.Utils;
using Synesthesia.Utils.Profiler;

namespace Synesthesia.Parsers.Model;

public static class ObjFileParser
{
    public static Action<string> LogFunc = Console.WriteLine;

    public static MeshTemplate Parse(Stream stream)
    {
        var timings = Timings.RentAndPush();
        var contents = stream.ReadString();

        var lines = contents.Split("\n");
        List<Vector3> vertices = [];
        List<Vector3> normals = [];
        List<Vector2> textureCoords = [];
        List<MeshFace> faces = [];

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

            // Vertex
            if (trimmed.StartsWith("v "))
            {
                var split = trimmed.Split(" ");
                var x = float.Parse(split[1], CultureInfo.InvariantCulture);
                var y = float.Parse(split[2], CultureInfo.InvariantCulture);
                var z = float.Parse(split[3], CultureInfo.InvariantCulture);
                vertices.Add(new Vector3(x, y, z));
            }
            else if (trimmed.StartsWith("vt ")) // Texture coordinates
            {
                var split = trimmed.Split(" ");
                var x = float.Parse(split[1], CultureInfo.InvariantCulture);
                var y = float.Parse(split[2], CultureInfo.InvariantCulture);
                textureCoords.Add(new Vector2(x, y));
            }
            else if (trimmed.StartsWith("vn ")) // Normals
            {
                var split = trimmed.Split(" ");
                var x = float.Parse(split[1], CultureInfo.InvariantCulture);
                var y = float.Parse(split[2], CultureInfo.InvariantCulture);
                var z = float.Parse(split[3], CultureInfo.InvariantCulture);
                normals.Add(new Vector3(x, y, z));
            }
            else if (trimmed.StartsWith('f'))
            {
                var faceDefinitions = trimmed.Split(" ");
                List<MeshFacePoint> points = [];

                foreach (var faceDefinition in faceDefinitions)
                {
                    if (faceDefinition.StartsWith('f')) continue;

                    var split = faceDefinition.Split("/");

                    int positionIndex;
                    int? uvIndex = null;
                    int? normalIndex = null;

                    switch (split.Length)
                    {
                        // 1
                        case 1:
                            positionIndex = int.Parse(split[0]) - 1;
                            break;
                        // 1/2
                        case 2:
                            positionIndex = int.Parse(split[0]) - 1;
                            uvIndex = int.Parse(split[1]) - 1;
                            break;
                        // 1//2 or 1/1/2
                        default:
                            positionIndex = int.Parse(split[0]) - 1;
                            uvIndex = split[1] == string.Empty ? null : int.Parse(split[1]) - 1;
                            normalIndex = int.Parse(split[2]) - 1;
                            break;
                    }

                    points.Add(new MeshFacePoint(positionIndex, uvIndex, normalIndex));
                }

                faces.Add(new MeshFace(points));
            }
        }

        List<MeshTemplate.Vertex> finalVertices = [];
        List<uint> finalIndices = [];
        Dictionary<(int, int, int), int> seenCorners = new();

        foreach (var face in faces)
        {
            // Fan triangulation loop
            for (int i = 1; i < face.Points.Count - 1; i++)
            {
                addCorner(face.Points[0], vertices, textureCoords, normals, finalVertices, finalIndices, seenCorners);
                addCorner(face.Points[i], vertices, textureCoords, normals, finalVertices, finalIndices, seenCorners);
                addCorner(face.Points[i + 1], vertices, textureCoords, normals, finalVertices, finalIndices, seenCorners);
            }
        }

        LogFunc.Invoke($"Parsed obj file in {timings.PopAndReturn()}ms");
        return new MeshTemplate(finalVertices, finalIndices);
    }

    private static void addCorner(
        MeshFacePoint corner,
        List<Vector3> positions,
        List<Vector2> textureCoords,
        List<Vector3> normals,
        List<MeshTemplate.Vertex> finalVertices,
        List<uint> finalIndices,
        Dictionary<(int, int, int), int> seenCorners)
    {
        var key = (corner.Position, corner.TexCoord ?? -1, corner.Normal ?? -1);

        if (!seenCorners.TryGetValue(key, out int finalIndex))
        {
            var pos = positions[corner.Position];
            var uv = corner.TexCoord.HasValue ? textureCoords[corner.TexCoord.Value] : Vector2.Zero;
            var nrm = corner.Normal.HasValue ? normals[corner.Normal.Value] : Vector3.UnitY;

            var vertex = new MeshTemplate.Vertex(pos, nrm, uv);

            finalIndex = finalVertices.Count;
            finalVertices.Add(vertex);
            seenCorners[key] = finalIndex;
        }

        finalIndices.Add((uint)finalIndex);
    }

    private record MeshFacePoint(int Position, int? TexCoord, int? Normal);

    private record MeshFace(List<MeshFacePoint> Points);
}
