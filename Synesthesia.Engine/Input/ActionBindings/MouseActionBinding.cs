// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Input.ActionBindings;

public record MouseActionBinding(MouseButton MouseButton) : IActionBinding
{
    public bool IsDown => MouseButton.IsDown();

    public override string ToString() => $"MouseActionBinding(MouseButton={MouseButton})";
}
