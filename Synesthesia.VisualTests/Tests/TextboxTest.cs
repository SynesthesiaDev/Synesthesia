// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Input;

namespace Synesthesia.VisualTests.Tests;

public class TextboxTest : VisualTest
{
    private DefaultTextbox textBox = null!;

    private const string testing_string = "testing";
    private const string long_string = "this is pretty long string that is actually really really long and did i mention its long";

    private string committedString = "";

    protected override void OnLoading()
    {
        Children =
        [
            textBox = new DefaultTextbox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 40)
            }
        ];

        AddAssert("Textbox is not focused", () => !textBox.Focused.Value);

        AddStep("Click on textbox", () => InputSimulator.SimulateClick(MouseButton.Left, textBox.GetScreenSpaceCenter()));

        AddAssert("Textbox is focused", () => textBox.Focused.Value);

        AddStep("Type test string", () => InputSimulator.SimulateTyping(testing_string, 500));

        AddWaitUntil("Wait until typed", () => textBox.Text.Value == testing_string, 500);

        AddAssert($"Text is \"{testing_string}\"", () => textBox.Text.Value == testing_string);

        AddStep("Select half", () =>
        {
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftShift, true);

            for (int i = 0; i < 4; i++) InputSimulator.SimulateKeyboardPress(KeyboardKey.Left);

            InputSimulator.SimulateKeyboard(KeyboardKey.LeftShift, false);
        });

        AddAssert("Textbox has selection", () => textBox.UnderlyingTextBox.HasSelection);

        AddAssert("Selection is 4", () => textBox.UnderlyingTextBox.SelectedText.Length == 4);

        AddStep("Change selection color", () => textBox.UnderlyingTextBox.SelectionColor.Value = Color.Pink);

        AddStep("Delete selection", () => InputSimulator.SimulateKeyboardPress(KeyboardKey.Backspace));

        AddAssert("Textbox doesn't have selection", () => !textBox.UnderlyingTextBox.HasSelection);

        AddAssert("Selection is 0", () => textBox.UnderlyingTextBox.SelectedText.Length == 0);

        AddStep("Click outside", () => InputSimulator.SimulateClick(MouseButton.Left, new Vector2(0, 0)));

        AddStep("Select all", () =>
        {
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, true);
            InputSimulator.SimulateKeyboardPress(KeyboardKey.A);
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, false);
        });

        AddAssert("Textbox doesn't have selection", () => !textBox.UnderlyingTextBox.HasSelection);

        AddAssert("Selection is 0", () => textBox.UnderlyingTextBox.SelectedText.Length == 0);

        AddStep("Click on textbox", () => InputSimulator.SimulateClick(MouseButton.Left, textBox.GetScreenSpaceCenter()));

        AddStep("Select all", () =>
        {
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, true);
            InputSimulator.SimulateKeyboardPress(KeyboardKey.A);
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, false);
        });

        AddAssert("Selection is 3", () => textBox.UnderlyingTextBox.SelectedText.Length == 3);

        AddStep("Type long string", () => InputSimulator.SimulateTyping(long_string, 2000));

        AddWaitUntil("Wait until typed", () => textBox.Text.Value == long_string, 2000);

        AddStep("Select all", () =>
        {
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, true);
            InputSimulator.SimulateKeyboardPress(KeyboardKey.A);
            InputSimulator.SimulateKeyboard(KeyboardKey.LeftControl, false);
        });

        AddStep("Click outside", () => InputSimulator.SimulateClick(MouseButton.Left, new Vector2(0, 0)));

        AddAssert("Textbox is not focused", () => !textBox.Focused.Value);

        AddAssert("Selection is cleared", () => !textBox.UnderlyingTextBox.HasSelection);

        AddStep("Click on textbox", () => InputSimulator.SimulateClick(MouseButton.Left, textBox.GetScreenSpaceCenter()));

        AddAssert("Textbox is focused", () => textBox.Focused.Value);

        AddStep("Press enter", () => InputSimulator.SimulateKeyboardPress(KeyboardKey.Enter));

        AddAssert("Textbox is not focused", () => !textBox.Focused.Value);

        AddAssert("Committed string is matching", () => committedString == long_string);
    }

    protected override void LoadComplete()
    {
        textBox.OnCommit.Subscribe(e => committedString = e);
        base.LoadComplete();
    }
}
