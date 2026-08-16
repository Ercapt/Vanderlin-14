using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Systems.RogueHud.Controls;

/// <summary>
/// Left-anchored layout panel that scales its children based on screen height,
/// preserving the tower's 160x480 virtual aspect ratio.
/// Art is always scaled to fit screen height, anchored to the left edge.
/// </summary>
public sealed class RogueHudOverlayPanel : LayoutContainer
{
    /// <summary>Virtual art size of the tower texture (160x480).</summary>
    public Vector2 VirtualSize { get; set; } = new Vector2(160, 480);

    private readonly Dictionary<Control, UIBox2> _virtualRects = new();

    public void SetVirtualRect(Control child, UIBox2 virtualRect)
    {
        _virtualRects[child] = virtualRect;
        RecalculateLayout();
    }

    /// <summary>
    /// Called when the panel is resized. Recalculates all child positions/sizes
    /// based on current screen height (scale to fit height, anchor to left).
    /// </summary>
    protected override void Resized()
    {
        base.Resized();
        RecalculateLayout();
    }

    public void RecalculateLayout()
    {
        if (VirtualSize.X <= 0 || VirtualSize.Y <= 0)
            return;

        var screenH = Size.Y;
        if (screenH <= 0)
            return;

        // Scale uniformly based on screen height (tower fills full height, anchored left)
        float scale = screenH / VirtualSize.Y;

        foreach (var (child, vr) in _virtualRects)
        {
            if (child.Parent != this)
                continue;

            var pos = new Vector2(vr.Left * scale, vr.Top * scale);
            var size = new Vector2(vr.Width * scale, vr.Height * scale);

            SetPosition(child, pos);
            child.SetSize = size;
        }
    }

    // Legacy compat — kept so existing code that calls these doesn't break
    public void BindToBackground(TextureRect background, Vector2 artVirtualSize)
    {
        VirtualSize = artVirtualSize;
    }

    public void UpdateFromBackground(TextureRect background)
    {
        // No-op: we now scale by screen height, not by TextureRect.Size
        RecalculateLayout();
    }
}
