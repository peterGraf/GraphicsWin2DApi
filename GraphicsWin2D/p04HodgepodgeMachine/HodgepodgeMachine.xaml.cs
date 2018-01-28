
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
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace p04HodgepodgeMachine
{
    /// <summary>
    /// The hodgepodge machine.
    /// 
    /// A kind of cellular automaton for time-spatially discrete simulations.
    /// See https://arxiv.org/pdf/1507.08783.pdf.
    /// 
    /// </summary>
    public sealed partial class HodgepodgeMachine : Page
    {
        private static int _width = 1080;
        private static int _height = 1080;
        private static int _height4 = 4 * _height;
        private static int _nBytes = _width * _height4;

        private int[,] _current = new int[_width, _height];
        private int[,] _next = new int[_width, _height];

        private int _nIteration;

        private int _maxState = 127; //  the maximal state level
        private double _k1 = 2; // the weight of the cell in the ignition state from the interval ( 0 , maxState )
        private double _k2 = 2; // the weight of the cell in the ignition state maxState
        private int _g = 1; //  the number of levels crossed in one simulation step

        private void CanvasResourcesCreate(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    //if (RandomBool())
                    {
                        _current[x, y] = _random.Next(_maxState + 1);
                        _next[x, y] = _random.Next(_maxState + 1);
                    }
                }
            }
        }

        private byte[] _buffer = new byte[_nBytes];

        private bool _restart = false;

        private void CanvasAnimatedDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            DrawEventCount();

            _nIteration++;
            const float usedDpi = 96.0f;

            if (_restart || _nIteration % 2048 == 0)
            {
                _restart = false;
                CanvasResourcesCreate(null, null);
                _g++;
            }

            bool oneState = true;
            byte theState = 0;

            for (int x = 0, bufferIndex = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++, bufferIndex += 4)
                {
                    byte state = (byte)(_next[x, y] = NextStateValue(_current, x, y));
                    if (x == 0 && y == 0)
                    {
                        theState = state;
                    }
                    else if (theState != state)
                    {
                        oneState = false;
                    }

                    switch (state % 16)
                    {
                        case 0:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 255;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 1:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 255;
                            _buffer[bufferIndex + 2] = 180;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 2:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 255;
                            _buffer[bufferIndex + 2] = 100;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 3:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 255;
                            _buffer[bufferIndex + 2] = 0;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 4:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 180;
                            _buffer[bufferIndex + 2] = 0;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 5:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 100;
                            _buffer[bufferIndex + 2] = 0;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 6:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 0;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 7:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 100;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 8:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 180;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 9:
                            _buffer[bufferIndex + 0] = 255;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 10:
                            _buffer[bufferIndex + 0] = 180;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 11:
                            _buffer[bufferIndex + 0] = 100;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 12:
                            _buffer[bufferIndex + 0] = 0;
                            _buffer[bufferIndex + 1] = 0;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 13:
                            _buffer[bufferIndex + 0] = 0;
                            _buffer[bufferIndex + 1] = 100;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        case 14:
                            _buffer[bufferIndex + 0] = 0;
                            _buffer[bufferIndex + 1] = 180;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                        default:
                            _buffer[bufferIndex + 0] = 0;
                            _buffer[bufferIndex + 1] = 255;
                            _buffer[bufferIndex + 2] = 255;
                            _buffer[bufferIndex + 3] = 255;
                            break;
                    }
                }
            }

            if (oneState)
            {
                _restart = true;
            }

            using (var bitmap = CanvasBitmap.CreateFromBytes(sender, _buffer, _width, _height, DirectXPixelFormat.B8G8R8A8UIntNormalized, usedDpi))
            {
                args.DrawingSession.DrawImage(bitmap, new Rect(0, 0, _width, _height), new Rect(0, 0, _width, _height), 1, CanvasImageInterpolation.NearestNeighbor);
            }

            args.DrawingSession.DrawText($"FPS {FramesPerSecond:F2}", new Vector2(10, 10), Colors.Black);
            args.DrawingSession.DrawText("Step " + _nIteration, new Vector2(10, 30), Colors.Black);
            args.DrawingSession.DrawText("G " + _g, new Vector2(10, 50), Colors.Black);

            var tmp = _next;
            _next = _current;
            _current = tmp;
        }

        #region Hodgepodge Machine

        private int SumOfNeighbors(int[,] rectangle, int x, int y, out int a, out int b)
        {
            int sum = 0;
            b = 0;
            a = 0;

            x += _width;
            y += _height;

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    //if (xOffset != 0 || yOffset != 0)
                    {
                        int state = rectangle[(x + xOffset) % _width, (y + yOffset) % _height];
                        sum += state;
                        if (state > 0)
                        {
                            if (state < _maxState)
                            {
                                a++;
                            }
                            else
                            {
                                b++;
                            }
                        }
                    }
                }
            }

            return sum;
        }

        private int NextStateValue(int[,] rectangle, int x, int y)
        {
            int nextState = 0;
            int state = rectangle[x, y];
            if (state == _maxState)
            {
                return nextState;
            }
            int sumOfNeighbors = SumOfNeighbors(rectangle, x, y, out int a, out int b);
            if (state == 0)
            {
                nextState = (int)(a / _k1) + (int)(b / _k2);
            }
            else if (a > 0)
            {
                nextState = sumOfNeighbors / (a) + _g;
            }

            if (nextState > _maxState)
            {
                return _maxState;
            }
            return nextState;
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

        public HodgepodgeMachine()
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
