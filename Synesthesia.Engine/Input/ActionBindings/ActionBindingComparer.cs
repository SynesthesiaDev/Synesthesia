// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Input.ActionBindings;

public class ActionBindingComparer : IEqualityComparer<IActionBinding>
{
    public bool Equals(IActionBinding? x, IActionBinding? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return x switch
        {
            KeyboardActionBinding k1 when y is KeyboardActionBinding k2 =>
                k1.Primary == k2.Primary &&
                k1.Modifiers.Length == k2.Modifiers.Length &&
                new HashSet<Key>(k1.Modifiers).SetEquals(k2.Modifiers),

            MouseActionBinding m1 when y is MouseActionBinding m2 =>
                m1.MouseButton == m2.MouseButton,

            CoordinatedActionBinding c1 when y is CoordinatedActionBinding c2 =>
                c1.ActionBindings.Length == c2.ActionBindings.Length &&
                c1.ActionBindings.All(comp1 => c2.ActionBindings.Any(comp2 => Equals(comp1, comp2))),

            _ => x.Equals(y)
        };
    }

    public int GetHashCode(IActionBinding obj)
    {
        var hash = new HashCode();
        hash.Add(obj.GetType());

        if (obj is KeyboardActionBinding k)
        {
            hash.Add(k.Primary);
            int modsHash = k.Modifiers.Aggregate(0, (current, m) => current ^ m.GetHashCode());
            hash.Add(modsHash);
        }
        else
        {
            hash.Add(obj.GetHashCode());
        }

        return hash.ToHashCode();
    }
}
