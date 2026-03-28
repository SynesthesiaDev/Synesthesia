// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Input.ActionBindings;

public record PlatformActionBinding(IList<IActionBinding> AlternativeBindings)
{
    public override string ToString()
    {
        return $"PlatformActionBinding(AlternativeBindings={AlternativeBindings.AsString()})";
    }

    public bool IsDown => AlternativeBindings.Any(b => b.IsDown);

    public PlatformActionBinding Register()
    {
        InputHandler.RegisterActionBinding(this);
        return this;
    }
}
