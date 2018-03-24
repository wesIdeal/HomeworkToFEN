using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media;

namespace HomeworkReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<char, System.Windows.Controls.Image> pieceMap;
        static char?[,] board = new char?[10, 10];
        enum Files
        {
            a = 1, b, c, d, e, f, g, h
        };
        public MainWindow()
        {
            InitializeComponent();

        }
        private System.Windows.Controls.Image CropImage(Image img, int x, int y, int hxw)
        {
            Image croppedImage = new Image();
            croppedImage.Width = hxw;
            croppedImage.Height = hxw;
            BitmapImage bi = Resources["Pieces"] as BitmapImage;
            // Create a CroppedBitmap based off of a xaml defined resource.
            var cb = new CroppedBitmap(
              bi,
               new Int32Rect(x, y, hxw, hxw));       //select region rect
            croppedImage.Source = cb;
            return croppedImage;
        }


        public override void EndInit()
        {
            base.EndInit();




            var image = GetImageFromResource();
            var pieces = new List<System.Windows.Controls.Image>();
            for (int row = 0; row < 2; row++)
            {
                for (var col = 0; col < 6; col++)
                {
                    var ci = CropImage(image, 60 * col, 60 * row, 60);
                    pieces.Add(ci);

                }
            }
            pieceMap = new Dictionary<char, System.Windows.Controls.Image>();
            pieceMap.Add('q', pieces[0]);
            pieceMap.Add('k', pieces[1]);
            pieceMap.Add('r', pieces[2]);
            pieceMap.Add('n', pieces[3]);
            pieceMap.Add('b', pieces[4]);
            pieceMap.Add('p', pieces[5]);
            pieceMap.Add('Q', pieces[6]);
            pieceMap.Add('K', pieces[7]);
            pieceMap.Add('R', pieces[8]);
            pieceMap.Add('N', pieces[9]);
            pieceMap.Add('B', pieces[10]);
            pieceMap.Add('P', pieces[11]);
            InitArray();

            rbBlack.Click += OnColorChanged;
            rbWhite.Click += OnColorChanged;
            btnCopy.Click += OnCopyClick;
            tbHomework.TextChanged += CleanText;
            btnProcess.Click += OnProcessClick;
            ChessBoard.SizeChanged += ChessBoardInitialized;
            CleanText(null, null);

        }

        private void ChessBoardInitialized(object sender, EventArgs e)
        {
            RepaintBoard(false);
        }



        private void CleanText(object sender, TextChangedEventArgs e)
        {
            var inputText = GetInputLines();

            tbHomework.Text = string.Join("\n", inputText);
            if (inputText.Count() >= 2)
            {
                OnProcessClick(null, null);
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbFen.Text);
        }

        private static void InitArray()
        {
            for (int f = 0; f <= 9; f++)
            {
                for (int r = 0; r <= 9; r++)
                {
                    board[f, r] = null;
                }
            }
        }

        private System.Windows.Controls.Image GetImageFromResource()
        {
            return (System.Windows.Controls.Image)Application.Current.Resources["Pieces"];

        }

        private void OnProcessClick(object sender, RoutedEventArgs e)
        {
            var lines = GetInputLines();
            var white = lines[0];
            var black = lines[1];
            InitArray();
            var whiteToMove = rbWhite.IsChecked.Value;
            var whiteArray = white.Replace(" ", "").Split(',');
            var blackArray = black.Replace(" ", "").Split(',');
            InputMoves(whiteArray, true);
            InputMoves(blackArray, false);
            var fen = BuildFEN(whiteToMove);
            if (lines.Count() > 2)
            {
                tbQuestion.Text = lines[2];
            }
            else
            {
                tbQuestion.Text = "";
            }
            tbFen.Text = fen;
            RepaintBoard(true);
        }

        public int BoxDimension
        {
            get
            {
                var cWidth = ChessBoard.ActualWidth;
                var cHeight = ChessBoard.ActualHeight;
                var offset = Convert.ToInt32(cWidth / 20);
                return Convert.ToInt32((cWidth - (offset * 2)) / 9);
            }
        }

        private void RepaintBoard(bool drawPieces = true)
        {
            ChessBoard.UpdateLayout();
            ChessBoard.Children.Clear();
            var cWidth = ChessBoard.ActualWidth;
            var cHeight = ChessBoard.ActualHeight;
            var offset = Convert.ToInt32(cWidth / 20);
            var boxDimension = BoxDimension;

            var darkSquareBrush = new SolidColorBrush(Colors.DarkGreen);
            var lightSquareBrush = new SolidColorBrush(Colors.LightGray);



            var borderPen = new System.Drawing.Pen(System.Drawing.Color.Black);

            var rVal = 0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {

                for (var r = 8; r > 0; r--)
                {
                    rVal++;
                    TextBlock tbRank = new TextBlock();
                    tbRank.Text = r.ToString();
                    ChessBoard.Children.Add(tbRank);
                    Canvas.SetTop(tbRank, offset + (boxDimension * ((rVal - 1))) + boxDimension / 2);
                    Canvas.SetLeft(tbRank, offset / 2);
                    for (var f = 1; f < 9; f++)
                    {
                        SolidColorBrush b = ((r + f) % 2 == 0) ? Brushes.DarkGreen : Brushes.LightGray;
                        var rectPoint = new System.Windows.Point(Convert.ToInt32(offset + (boxDimension * (f - 1))), Convert.ToInt32(offset + (boxDimension * (rVal - 1))));
                        var rect = new System.Windows.Shapes.Rectangle()
                        {
                            Stroke = b,
                            Fill = b,
                            StrokeThickness = 1,
                            Height = boxDimension,
                            Width = boxDimension,

                        };

                        Canvas.SetTop(rect, rectPoint.Y);
                        Canvas.SetLeft(rect, rectPoint.X);
                        ChessBoard.Children.Add(rect);
                        var piece = board[f, r];
                        if (drawPieces && piece.HasValue)
                        {
                            var white = Char.IsUpper(piece.Value);

                            var image = new Image() { Source = pieceMap[piece.Value].Source, Stretch = Stretch.Fill, StretchDirection = StretchDirection.Both, Width = boxDimension, Height = boxDimension };

                            //drawingContext.DrawImage(ImageSourceForBitmap( pieceMap[piece.Value]), new Rect(rectPoint, new System.Windows.Size(boxDimension,boxDimension)));


                            ChessBoard.Children.Add(image);
                            Canvas.SetLeft(image, rectPoint.X);
                            Canvas.SetTop(image, rectPoint.Y);
                        }
                    }

                }
            }

            for (var foot = 1; foot < 9; foot++)
            {
                var c = (char)((int)'A' + (foot - 1));
                var rectPoint = new System.Windows.Point(Convert.ToInt32(offset + (boxDimension * (foot - 1))), Convert.ToInt32((boxDimension * 8)) + offset);
                TextBlock tbFile = new TextBlock();
                tbFile.Text = c.ToString();
                tbFile.Height = boxDimension / 2;
                tbFile.Width = boxDimension;
                tbFile.TextAlignment = TextAlignment.Center;

                // var pt = new System.Drawing.Point((offset + (boxDimension * (foot - 1))) + (boxDimension / 2), offset + (boxDimension * (8)))
                Canvas.SetTop(tbFile, rectPoint.Y);
                Canvas.SetLeft(tbFile, rectPoint.X);


                ChessBoard.Children.Add(tbFile);



            }
            tbHomework.Focus();
            tbHomework.SelectAll();

        }




        private static void InputMoves(string[] pieces, bool isWhite)
        {
            foreach (var piece in pieces)
            {
                char? interprettedPiece = null;
                int? file = null;
                int? rank = null;
                if (char.IsUpper(piece[0]))
                {
                    interprettedPiece = isWhite ? Char.ToUpper(piece[0]) : Char.ToLower(piece[0]);
                    file = (int)Enum.Parse(typeof(Files), piece[1].ToString());
                    rank = int.Parse(piece[2].ToString());
                }
                else
                {
                    interprettedPiece = isWhite ? 'P' : 'p';
                    file = (int)Enum.Parse(typeof(Files), piece[0].ToString());
                    rank = int.Parse(piece[1].ToString());
                }
                if (interprettedPiece.HasValue)
                    board[file.Value, rank.Value] = interprettedPiece.Value;
            }
        }

        private static string BuildFEN(bool whiteToMove)
        {
            StringBuilder sb = new StringBuilder();
            for (int r = 8; r > 0; r--)
            {
                int nullCounter = 0;
                for (int f = 1; f <= 8; f++)
                {
                    var piece = board[f, r];

                    if (piece == null)
                    {
                        nullCounter++;
                    }
                    else
                    {
                        if (nullCounter != 0)
                        {
                            sb.Append(nullCounter.ToString());
                            nullCounter = 0;
                        }

                        sb.Append(piece);
                    }
                }
                if (nullCounter != 0) sb.Append(nullCounter);
                nullCounter = 0;
                if (r != 1) sb.Append('/');
            }
            sb.Append(" " + (whiteToMove ? "w" : "b") + " - - ");
            sb.Append(!whiteToMove ? "1 1" : "0 1");
            return sb.ToString();
        }

        private string[] GetInputLines()
        {
            List<string> rv = new List<string>();
            var txt = tbHomework.Text;
            var lines = txt.Replace("\r", "").Split('\n');
            var count = 0;
            foreach (var line in lines)
            {
                count++;
                var cleanerLine = line.Replace(">", "").Trim();
                if (cleanerLine.Length > 0)
                {
                    if (count <= 2)
                    {
                        rv.Add(cleanerLine.Replace(" ", ""));
                    }
                    else
                    {
                        rv.Add(cleanerLine);
                    } 
                }
            }
            return rv.ToArray();
        }

        private void OnColorChanged(object sender, RoutedEventArgs e)
        {
            if (GetInputLines().Count() >= 2)
            {
                OnProcessClick(sender, e);
            }
        }


    }
}
