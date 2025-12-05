
using Microsoft.AspNetCore.Components;
namespace CleanCCM.SharedComponents.Models;

public class TkCarrouselItem
{
    /// <summary>
    /// URL của hình ảnh
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Văn bản thay thế cho hình ảnh (accessibility)
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Nội dung tùy chỉnh của slide (thay thế cho ImageUrl)
    /// </summary>
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Tiêu đề hiển thị trong overlay
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Mô tả hiển thị trong overlay
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Văn bản nút bấm trong overlay
    /// </summary>
    public string? ButtonText { get; set; }

    /// <summary>
    /// Hành động khi nhấn nút trong overlay
    /// </summary>
    public Action? ButtonAction { get; set; }

    /// <summary>
    /// Nội dung overlay tùy chỉnh (nếu không dùng Title/Description/Button)
    /// </summary>
    public RenderFragment? OverlayContent { get; set; }

    /// <summary>
    /// Vị trí của overlay: TopLeft, Top, TopRight, Center, BottomLeft, Bottom, BottomRight
    /// </summary>
    public string OverlayPosition { get; set; } = "BottomLeft";

    /// <summary>
    /// Nếu true, overlay sẽ có nền tối đậm hơn
    /// </summary>
    public bool OverlayDark { get; set; } = false;

    /// <summary>
    /// Link đích khi click vào slide
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Target của link (_blank, _self, etc.)
    /// </summary>
    public string LinkTarget { get; set; } = "_self";
}