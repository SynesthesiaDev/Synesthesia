// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Commands;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

public class RenderQueue
{
    private readonly List<DrawMeshCommand> commands = [];

    private readonly Dictionary<(ulong, int), RenderBatch> batches = new();

    public void Submit(DrawMeshCommand renderCommand)
    {
        commands.Add(renderCommand);
    }

    public IEnumerable<RenderBatch> GetBatches()
    {
        batches.Clear();

        foreach (ref DrawMeshCommand command in CollectionsMarshal.AsSpan(commands))
        {
            var key = (command.Mesh.Handle, command.Material.GetBatchKey());

            if (!batches.TryGetValue(key, out var batch))
            {
                batch = new RenderBatch(command.Mesh, command.Material);
                batches[key] = batch;
            }

            batch.Instances.Add(command.WorldMatrix);
        }

        return batches.Values;
    }

    public void Clear()
    {
        commands.Clear();
        batches.Clear();
    }
}
