using System;
using System.Runtime.InteropServices;

namespace Gamma.Common
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        private int _left;
        private int _top;
        private int _right;
        private int _bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rect)
            {
                var rect = (Rect) obj;

                return rect._bottom == _bottom &&
                       rect._left == _left &&
                       rect._right == _right &&
                       rect._top == _top;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _bottom.GetHashCode() ^
                   _left.GetHashCode() ^
                   _right.GetHashCode() ^
                   _top.GetHashCode();
        }

        public static bool operator ==(Rect a, Rect b)
        {
            return a._bottom == b._bottom &&
                   a._left == b._left &&
                   a._right == b._right &&
                   a._top == b._top;
        }

        public static bool operator !=(Rect a, Rect b)
        {
            return !(a == b);
        }

        public int Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public int Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public int Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }
    }
}