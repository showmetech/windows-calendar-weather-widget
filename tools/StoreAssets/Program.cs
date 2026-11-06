using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
class Program {
 [STAThread] static void Main(string[] args){Directory.CreateDirectory(args[1]);var source=new BitmapImage(new Uri(Path.GetFullPath(args[0])));foreach(var size in new[]{44,50,150,310}){var visual=new DrawingVisual();using(var drawing=visual.RenderOpen())drawing.DrawImage(source,new Rect(0,0,size,size));var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var output=File.Create(Path.Combine(args[1],"Logo"+size+".png"));png.Save(output);}}
}
