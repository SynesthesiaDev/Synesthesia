// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Input.ActionBindings;

public class ActionBindingBuilder
{
    private readonly List<IActionBinding> bindings = [];

    public ActionBindingBuilder AddKeyboard(Key primary, params Key[] modifiers)
    {
        var keyboardActionBinding = new KeyboardActionBinding(primary, modifiers);
        bindings.Add(keyboardActionBinding);
        return this;
    }

    public ActionBindingBuilder AddMouse(MouseButton mouseButton)
    {
        var mouseActionBinding = new MouseActionBinding(mouseButton);
        bindings.Add(mouseActionBinding);
        return this;
    }

    public ActionBindingBuilder AddCoordinated(Action<ActionBindingBuilder> coordinatedBuilder)
    {
        var builder = new ActionBindingBuilder();
        coordinatedBuilder.Invoke(builder);
        bindings.Add(new CoordinatedActionBinding(builder.GetAsList().ToArray()));
        return this;
    }

    public PlatformActionBinding Build() => new(bindings);

    public IList<IActionBinding> GetAsList() => bindings;
}
