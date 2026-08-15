using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Systems.RogueHud.Controls;

/// <summary>
/// A layout control that scales child controls in a virtual 640x480 resolution space.
/// Keeps buttons, targeting doll, inventory slots, and viewport perfectly attached to the gothic frame.
/// </summary>
public sealed class RogueHudOverlayPanel : Control
{
    public Vector2 VirtualSize = new Vector2(640, 480);

    private readonly Dictionary<Control, UIBox2> _virtualRects = new();

    public void SetVirtualRect(Control child, UIBox2 virtualRect)
    {
        _virtualRects[child] = virtualRect;
        RecalculateLayout();
    }

    protected override void Resized()
    {
        base.Resized();
        RecalculateLayout();
    }

    public void RecalculateLayout()
    {
        var actual = Size;
        if (actual.X <= 0 || actual.Y <= 0)
            return;

        float scaleX = actual.X / VirtualSize.X;
        float scaleY = actual.Y / VirtualSize.Y;
        float scale = Math.Min(scaleX, scaleY);

        var offset = Vector2.Zero;

        foreach (var (child, vr) in _virtualRects)
        {
            if (child.Parent != this)
                continue;

            var pos = offset + new Vector2(vr.Left, vr.Top) * scale;
            var size = new Vector2(vr.Width, vr.Height) * scale;

            LayoutContainer.SetPosition(child, pos);
            child.SetSize = size;
        }
    }
}
