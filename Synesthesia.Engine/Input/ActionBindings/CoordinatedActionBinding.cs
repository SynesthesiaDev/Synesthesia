// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Input.ActionBindings;

public record CoordinatedActionBinding(params IActionBinding[] ActionBindings) : IActionBinding
{
    public bool IsDown => ActionBindings.Length > 0 && ActionBindings.All(b => b.IsDown);

    public override string ToString() => $"CoordinatedActionBinding(ActionBindings={ActionBindings.AsString()})";
}
