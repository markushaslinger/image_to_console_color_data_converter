using System.Collections.Frozen;
using System.Text;
using ColorMine.ColorSpaces;

namespace ImageForConsoleConverter.Pages;

public partial class Index
{
    private static readonly FrozenDictionary<ConsoleColor, Rgb> consoleColorToRgb = new Dictionary<ConsoleColor, Rgb>
    {
        { ConsoleColor.Black, new Rgb { R = 0, G = 0, B = 0 } },
        { ConsoleColor.DarkBlue, new Rgb { R = 0, G = 0, B = 128 } },
        { ConsoleColor.DarkGreen, new Rgb { R = 0, G = 128, B = 0 } },
        { ConsoleColor.DarkCyan, new Rgb { R = 0, G = 128, B = 128 } },
        { ConsoleColor.DarkRed, new Rgb { R = 128, G = 0, B = 0 } },
        { ConsoleColor.DarkMagenta, new Rgb { R = 128, G = 0, B = 128 } },
        { ConsoleColor.DarkYellow, new Rgb { R = 128, G = 128, B = 0 } },
        { ConsoleColor.Gray, new Rgb { R = 128, G = 128, B = 128 } },
        { ConsoleColor.DarkGray, new Rgb { R = 192, G = 192, B = 192 } },
        { ConsoleColor.Blue, new Rgb { R = 0, G = 0, B = 255 } },
        { ConsoleColor.Green, new Rgb { R = 0, G = 255, B = 0 } },
        { ConsoleColor.Cyan, new Rgb { R = 0, G = 255, B = 255 } },
        { ConsoleColor.Red, new Rgb { R = 255, G = 0, B = 0 } },
        { ConsoleColor.Magenta, new Rgb { R = 255, G = 0, B = 255 } },
        { ConsoleColor.Yellow, new Rgb { R = 255, G = 255, B = 0 } },
        { ConsoleColor.White, new Rgb { R = 255, G = 255, B = 255 } }
    }.ToFrozenDictionary();

    private Image<Rgba32>? _rawImage;
    private int _width = 40;

    private static int[,] ConvertImage(Image<Rgba32> image, int desiredWidth)
    {
        try
        {
            var aspectRatio = image.Width / (double) image.Height;
            var desiredHeight = (int) (desiredWidth / aspectRatio);

            // Resize image
            image.Mutate(x => x.Resize(desiredWidth, desiredHeight));

            // Create array to hold color data
            var pixelColors = new int[desiredWidth, desiredHeight];

            // Map colors to closest ConsoleColor
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixelColor = image[x, y];
                    var closestConsoleColor = Enum.GetValues(typeof(ConsoleColor))
                                                  .Cast<ConsoleColor>()
                                                  .MinBy(c =>
                                                             ColorDistance(new Rgb { R = pixelColor.R, G = pixelColor.G, B = pixelColor.B },
                                                                           ConsoleColorToRgb(c)));
                    pixelColors[x, y] = (int) closestConsoleColor;
                }
            }

            return pixelColors;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return new int[0, 0];
        }
    }

    private static double ColorDistance(IRgb color1, IRgb color2)
    {
        var redDifference = color1.R - color2.R;
        var greenDifference = color1.G - color2.G;
        var blueDifference = color1.B - color2.B;

        return Math.Sqrt(redDifference * redDifference + greenDifference * greenDifference +
                         blueDifference * blueDifference);
    }

    private static Rgb ConsoleColorToRgb(ConsoleColor color) =>
        consoleColorToRgb.TryGetValue(color, out var rgb)
            ? rgb
            : new Rgb { R = 0, G = 0, B = 0 }; // Default color if not found

    private void HandleImageUploaded(Image<Rgba32> uploadedImage)
    {
        _rawImage?.Dispose();
        _rawImage = uploadedImage;
        OutputImageCsv();
    }

    private void OutputImageCsv()
    {
        if (_rawImage is null)
        {
            return;
        }

        var converted = ConvertImage(_rawImage, _width);
        _csvRows = ConvertArrayToCsv(converted);
        StateHasChanged();
    }

    private static IEnumerable<string> ConvertArrayToCsv(int[,] array)
    {
        var width = array.GetLength(0);
        var height = array.GetLength(1);
        var rows = new string[height];

        for (var i = 0; i < height; i++)
        {
            var csv = new StringBuilder();
            for (var j = 0; j < width; j++)
            {
                csv.Append($"{array[j, i]:00}");
                if (j < width - 1)
                {
                    csv.Append(',');
                }
            }

            rows[i] = csv.ToString();
        }

        return rows;
    }

    private void HandleWidthChanged(int newWidth)
    {
        _width = newWidth;
        OutputImageCsv();
    }
}
