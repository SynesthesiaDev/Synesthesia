// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Input;

public class KeyCombo(Key primary, params Key[] modifiers)
{
    public readonly Key Primary = primary;
    public readonly Key[] Modifiers = modifiers;

    public string StringRepresentation => Modifiers.Aggregate($"{Primary}", (current, modifier) => current + $" + {modifier}");

    public override string ToString()
    {
        return $"KeyCombo(Primary{Primary}, Modifiers={Modifiers})";
    }
}
