// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Input.ActionBindings;

public record KeyboardActionBinding(Key Primary, params Key[] Modifiers) : IActionBinding
{
    public bool IsDown => Primary.IsDown() && Modifiers.All(k => k.IsDown());

    public int Complexity => 1 + Modifiers.Length;

    public override string ToString() => $"KeyboardActionBinding(Primary={Primary}, Modifiers={Modifiers.AsString()})";
}
