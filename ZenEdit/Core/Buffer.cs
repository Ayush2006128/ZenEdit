using System;

namespace ZenEdit.core
{
    public enum BufferSource
    {
        Original,
        Add
    }

    public struct Piece
    {
        public BufferSource Source;
        public int Start;
        public int Length;

        public Piece(BufferSource source, int start, int length)
        {
            Source = source;
            Start = start;
            Length = length;
        }
    }
}
