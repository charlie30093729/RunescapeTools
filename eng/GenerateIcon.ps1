param(
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\src\RunescapePriceChecker.Wpf\Assets\ge-ledger.ico')
)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeIconMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(IntPtr handle);
}
"@

$bitmap = [System.Drawing.Bitmap]::new(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

function Add-RoundedRectangle([System.Drawing.Drawing2D.GraphicsPath] $path, [int] $x, [int] $y, [int] $size, [int] $radius) {
    $diameter = $radius * 2
    $path.AddArc($x, $y, $diameter, $diameter, 180, 90)
    $path.AddArc($x + $size - $diameter, $y, $diameter, $diameter, 270, 90)
    $path.AddArc($x + $size - $diameter, $y + $size - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($x, $y + $size - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
}

try {
    $outer = [System.Drawing.Drawing2D.GraphicsPath]::new()
    Add-RoundedRectangle $outer 0 0 256 42
    $graphics.FillPath([System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#1f2420')), $outer)

    $gold = [System.Drawing.Drawing2D.GraphicsPath]::new()
    Add-RoundedRectangle $gold 25 25 206 29
    $graphics.FillPath([System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#a16f1d')), $gold)

    $paper = [System.Drawing.Drawing2D.GraphicsPath]::new()
    Add-RoundedRectangle $paper 39 39 178 21
    $graphics.FillPath([System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#f3eedf')), $paper)

    $font = [System.Drawing.Font]::new('Georgia', 66, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $format = [System.Drawing.StringFormat]::new()
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $graphics.DrawString('GE', $font, [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#27241f')), [System.Drawing.RectangleF]::new(39, 49, 178, 126), $format)

    $pen = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#a16f1d'), 7)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($pen, 70, 174, 186, 174)

    $handle = $bitmap.GetHicon()
    try {
        $icon = [System.Drawing.Icon]::FromHandle($handle)
        $absoluteOutput = [System.IO.Path]::GetFullPath($OutputPath)
        [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($absoluteOutput)) | Out-Null
        $stream = [System.IO.File]::Create($absoluteOutput)
        try { $icon.Save($stream) } finally { $stream.Dispose(); $icon.Dispose() }
    }
    finally {
        [NativeIconMethods]::DestroyIcon($handle) | Out-Null
    }
}
finally {
    $graphics.Dispose()
    $bitmap.Dispose()
}
