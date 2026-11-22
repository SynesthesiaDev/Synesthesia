using System.Numerics;
using Synesthesia.Engine.Graphics.Commands;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

public class RenderQueue
{
    private readonly List<DrawMeshCommand> commands = [];
    private readonly Dictionary<(ulong, int), RenderBatch> batchCache = [];
    private readonly List<RenderBatch> activeBatches = [];

    public void Submit(DrawMeshCommand renderCommand)
    {
        commands.Add(renderCommand);
    }

    public List<RenderBatch> GetBatches(Vector3? cameraPosition = null)
    {
        foreach (ref DrawMeshCommand command in CollectionsMarshal.AsSpan(commands))
        {
            var key = (command.Mesh.StaticBatchId, command.Material.GetBatchKey());

            if (!batchCache.TryGetValue(key, out var batch))
            {
                batch = new RenderBatch
                {
                    Material = command.Material,
                    Mesh = command.Mesh
                };
                batchCache[key] = batch;
            }

            if (batch.Instances.Count == 0)
            {
                activeBatches.Add(batch);
            }

            batch.Instances.Add(command.WorldMatrix);
        }

        sortBatches(cameraPosition);
        return activeBatches;
    }

    private void sortBatches(Vector3? cameraPosition)
    {
        if (cameraPosition is { } cam)
        {
            activeBatches.Sort((a, b) =>
            {
                bool aT = a.Material.IsTransparent;
                bool bT = b.Material.IsTransparent;

                // Opaque always before transparent
                if (aT != bT) return aT ? 1 : -1;

                if (!aT) return 0;

                // back-to-front for transparent and nonstransparent
                float aD = Vector3.DistanceSquared(a.Centroid, cam);
                float bD = Vector3.DistanceSquared(b.Centroid, cam);
                return bD.CompareTo(aD);
            });
        }
        else
        {
            // no cam, at least split opaque from transparent
            activeBatches.Sort((a, b) =>
            {
                bool aT = a.Material.IsTransparent;
                bool bT = b.Material.IsTransparent;
                if (aT == bT) return 0;
                return aT ? 1 : -1;
            });
        }
    }

    public void Clear()
    {
        commands.Clear();

        foreach (ref RenderBatch batch in CollectionsMarshal.AsSpan(activeBatches))
        {
            batch.Instances.Clear();
        }
        activeBatches.Clear();
        batchCache.Clear();
    }
}
