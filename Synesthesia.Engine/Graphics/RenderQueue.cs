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

    public List<RenderBatch> GetBatches()
    {
        foreach (ref DrawMeshCommand command in CollectionsMarshal.AsSpan(commands))
        {
            var key = (command.Mesh.Handle, command.Material.GetBatchKey());

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

        return activeBatches;
    }

    public void Clear()
    {
        commands.Clear();

        foreach (ref RenderBatch batch in CollectionsMarshal.AsSpan(activeBatches))
        {
            batch.Instances.Clear();
        }
        activeBatches.Clear();
    }
}
