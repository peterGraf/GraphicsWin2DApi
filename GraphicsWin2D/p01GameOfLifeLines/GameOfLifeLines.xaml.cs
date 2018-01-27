/*
This software is a part of Peter Graf's GraphicsWin2DApi.

GraphicsWin2DApi is hosted on GitHub,
see http://github.com/peterGraf/GraphicsWin2DApi.

GraphicsWin2DApi is an open source test project for Microsoft's Win2D Windows Runtime API,
see http://github.com/Microsoft/Win2D.

For more information on the author Peter Graf,
see http://www.mission-base.com.

Copyright (c) 2018 Peter Graf. All rights reserved.

MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to
deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.  
*/
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using System;
using System.Numerics;
using Microsoft.Graphics.Canvas.UI;
using System.Linq;

namespace p01GameOfLifeLines
{
    /// <summary>
    /// Game of life.
    /// 
    /// This version directly draws lines for the pixels onto the canvas
    /// with many calls to DrawLine.
    /// 
    /// </summary>
    public sealed partial class GameOfLifeLines : Page
    {
        private static int _width = 1080;
        private static int _height = 1080;

        private bool[,] _current = new bool[_width, _height];
        private bool[,] _next = new bool[_width, _height];

        private int _nIteration;
        private int _nPixel;
        private int _minimum = int.MaxValue;
        private int _nLines;

        private void CanvasResourcesCreate(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            for (int x = _width / 4; x < _width * 3 / 4; x++)
            {
                for (int y = _height / 4; y < _height * 3 / 4; y++)
                {
                    if (RandomBool())
                    {
                        _current[x, y] = true;
                    }
                }
            }
        }

        private void CanvasAnimatedDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            DrawEventCount();

            // Add one live cell per draw, just to keep it alive forever
            //
            _current[_random.Next(_width), _random.Next(_height)] = true;

            _nIteration++;
            _nPixel = 0;
            _nLines = 0;

            int nPixelsToDraw = 0;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_next[x, y] = NextLiveValue(_current, x, y))
                    {
                        _nPixel++;
                        nPixelsToDraw++;
                    }
                    else if (nPixelsToDraw > 0)
                    {
                        _nLines++;
                        args.DrawingSession.DrawLine(new Vector2(x, y - nPixelsToDraw), new Vector2(x, y), Colors.Black);
                        nPixelsToDraw = 0;
                    }
                }
                if (nPixelsToDraw > 0)
                {
                    _nLines++;
                    args.DrawingSession.DrawLine(new Vector2(x, _height - nPixelsToDraw), new Vector2(x, _height), Colors.Black);
                    nPixelsToDraw = 0;
                }
            }
            if (_nPixel < _minimum)
            {
                _minimum = _nPixel;
            }

            args.DrawingSession.DrawText($"FPS {FramesPerSecond:F2}", new Vector2(10, 10), Colors.Black);
            args.DrawingSession.DrawText("Step " + _nIteration, new Vector2(10, 30), Colors.Black);
            args.DrawingSession.DrawText("Pixel " + _nPixel, new Vector2(10, 50), Colors.Black);
            args.DrawingSession.DrawText("Minimum " + _minimum, new Vector2(10, 70), Colors.Black);
            args.DrawingSession.DrawText("Lines " + _nLines, new Vector2(10, 90), Colors.Black);

            var tmp = _next;
            _next = _current;
            _current = tmp;
        }

        #region Game of Life

        private static int LiveNeighborsCount(bool[,] rectangle, int x, int y)
        {
            int n = 0;
            x += _width;
            y += _height;

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    if (xOffset != 0 || yOffset != 0)
                    {
                        if (rectangle[(x + xOffset) % _width, (y + yOffset) % _height])
                        {
                            if (++n > 3)
                            {
                                return n;
                            }
                        }
                    }
                }
            }
            return n;
        }

        private static bool NextLiveValue(bool[,] rectangle, int x, int y)
        {
            int n = LiveNeighborsCount(rectangle, x, y);
            if (n == 2)
            {
                return rectangle[x, y];
            }
            return n == 3;
        }

        #endregion

        #region Random

        private Random _random = new Random((int)(DateTime.Now.Ticks / 10000L));

        private bool RandomBool()
        {
            return _random.Next(2) != 0;
        }

        #endregion

        #region FramesPerSecond

        private static DateTime _theBeginning = new DateTime(2018, 1, 1);
        private int[] _drawEventsPerSecond = new int[5];
        private int _lastDrawEventSecond = -1;

        private void DrawEventCount()
        {
            int second = (int)new TimeSpan(DateTime.Now.Ticks - _theBeginning.Ticks).TotalSeconds;
            if (second > _lastDrawEventSecond)
            {
                if (second - _lastDrawEventSecond >= _drawEventsPerSecond.Length)
                {
                    for (int i = 0; i < _drawEventsPerSecond.Length; i++)
                    {
                        _drawEventsPerSecond[i] = 0;
                    }
                    _lastDrawEventSecond = second;
                }
                for (int i = _lastDrawEventSecond + 1; i <= second; i++)
                {
                    _drawEventsPerSecond[i % _drawEventsPerSecond.Length] = 0;
                }
                _lastDrawEventSecond = second;
            }
            _drawEventsPerSecond[second % _drawEventsPerSecond.Length] += 1;
        }

        private double FramesPerSecond
        {
            get
            {
                return _drawEventsPerSecond.Sum() / (float)_drawEventsPerSecond.Length;
            }
        }

        #endregion

        #region Constructor

        public GameOfLifeLines()
        {
            InitializeComponent();
        }

        public void PageUnloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        #endregion
    }
}
