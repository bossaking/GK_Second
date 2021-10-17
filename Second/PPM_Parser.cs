using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using FastBitmapLib;

namespace Second
{
    public class PpmParser
    {
        private int _width, _height, _maxCount;
        private Bitmap _bitmap;
        private StringBuilder _stringBuilder;
        private Color[] _colors;
        private int _colorIndex;
        private const int BufferSize = 8388608; // 8 MB
        private char[] _data;
        private byte[] _imageData;
        private int _r, _g, _b;
        private bool _rB, _gB;

        public Bitmap StartParse(string path)
        {
            if (!ReadHeaders(path))
            {
                return null;
            }

            var fb = new FastBitmap(_bitmap);
            fb.Lock();
            var col = 0;
            for (var i = 0; i < _bitmap.Height; i++)
            {
                for (var j = 0; j < _bitmap.Width; j++)
                {
                    fb.SetPixel(j, i, _colors[col]);
                    col++;
                }
            }

            fb.Unlock();

            return _bitmap;
        }

        private bool ReadHeaders(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                var format = new string(reader.ReadChars(2));
                if (!format.Equals("P3") && !format.Equals("P6"))
                {
                    MessageBox.Show("Error", "Cannot parse file format", MessageBoxButton.OK);
                    return false;
                }

                _stringBuilder = new StringBuilder();
                while (true)
                {
                    if (_maxCount != 0)
                    {
                        _colors = new Color[_width * _height];
                        _bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                        switch (format)
                        {
                            case "P3":
                                ReadP3(reader);
                                break;
                            case "P6":
                                ReadP6(reader);
                                break;
                        }

                        return true;
                    }

                    var value = reader.ReadByte();
                    if (value == 35)
                    {
                        GetHeaderFromString();
                        while (value != 10)
                        {
                            value = reader.ReadByte();
                        }
                    }
                    else if (value >= 48 && value <= 57)
                    {
                        _stringBuilder.Append(Convert.ToChar(value));
                    }
                    else
                    {
                        GetHeaderFromString();
                    }
                }
            }
        }

        private void ReadP3(BinaryReader reader)
        {
            _stringBuilder.Clear();

            while (true)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position == 0) return;

                _imageData = new byte[BufferSize]; //allocate 8mb of RAM

                var charsLeft = (int) (reader.BaseStream.Length - reader.BaseStream.Position);
                _data = reader.ReadChars(charsLeft > BufferSize ? BufferSize : charsLeft);

                var index = 0;
                for (var i = 0; i < _data.Length; i++)
                {
                    if (_data[i].Equals('#'))
                    {
                        GetColorFromString(ref index);
                        while (_data[i] != '\n')
                        {
                            i++;
                        }
                    }
                    else if (char.IsWhiteSpace(_data[i]))
                    {
                        GetColorFromString(ref index);
                    }
                    else
                    {
                        _stringBuilder.Append(_data[i]);
                    }
                }
                CreateBitmapOffSize(_imageData, index);
            }
        }
        
        private void ReadP6(BinaryReader reader)
        {
            while (true)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position == 0) return;
                var charsLeft = (int) (reader.BaseStream.Length - reader.BaseStream.Position);
                var index = charsLeft > BufferSize ? BufferSize : charsLeft;
                _imageData = reader.ReadBytes(index);
                CreateBitmapOffSize(_imageData, index);
            }
        }

        private void GetColorFromString(ref int index)
        {
            if (_stringBuilder.Length <= 0) return;
            if (_maxCount > 255)
            {
                _imageData[index] = (byte) (convert(_stringBuilder.ToString()) >> 8);
            }
            else
            {
                _imageData[index] = (byte) convert(_stringBuilder.ToString());
            }

            index++;
            _stringBuilder.Clear();
        }

        private void GetHeaderFromString()
        {
            if (_stringBuilder.Length <= 0) return;
            if (_width == 0)
            {
                _width = convert(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
            else if (_height == 0)
            {
                _height = convert(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
            else if (_maxCount == 0)
            {
                _maxCount = convert(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }
        }

        private int convert(string str)
        {
            var total = 0;
            var y = str.Aggregate(0, (current, t) => current * 10 + (t - '0'));
            total += y;
            return total;
        }

        private void CreateBitmapOffSize(IReadOnlyList<byte> imageData, int index)
        {
            for (var i = 0; i < index;)
            {
                if (!_rB && i != index)
                {
                    _rB = true;
                    _r = imageData[i];
                    i++;
                }

                if (!_gB && i != index)
                {
                    _gB = true;
                    _g = imageData[i];
                    i++;
                }

                if (i == index) break;
                _b = imageData[i];
                i++;
                if(_colorIndex == _colors.Length) return;
                _colors[_colorIndex] = Color.FromArgb(_r, _g, _b);
                _colorIndex++;
                _rB = false;
                _gB = false;
            }
        }
    }
}