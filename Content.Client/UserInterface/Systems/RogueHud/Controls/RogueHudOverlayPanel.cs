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

        var screenW = Size.X;
        var screenH = Size.Y;
        Logger.Info($"[OverlayPanel] RecalculateLayout called. Panel.Size={Size}, VirtualSize={VirtualSize}");
        if (screenH <= 0 || screenW <= 0)
        {
            Logger.Info("[OverlayPanel] Size <= 0 — skipping layout pass.");
            return;
        }

        float scaleX = screenW / VirtualSize.X;
        float scaleY = screenH / VirtualSize.Y;

        foreach (var (child, vr) in _virtualRects)
        {
            if (child.Parent != this)
                continue;

            var pos = new Vector2(vr.Left * scaleX, vr.Top * scaleY);
            var size = new Vector2(vr.Width * scaleX, vr.Height * scaleY);

            SetPosition(child, pos);
            child.SetSize = size;
            child.InvalidateMeasure();
            child.InvalidateArrange();
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
