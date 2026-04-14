using System;
using System.Collections.Generic;
using System.Text;

namespace ZenEdit.core
{
    public class PieceTable
    {
        private readonly string _originalBuffer;
        private readonly StringBuilder _addBuffer;
        private readonly List<Piece> _pieces;

        // The total number of characters in the buffer
        public int TotalLength { get; private set; }

        public PieceTable(string initialText)
        {
            _originalBuffer = initialText ?? string.Empty;
            _addBuffer = new StringBuilder();
            _pieces = new List<Piece>();
            TotalLength = _originalBuffer.Length;

            if (TotalLength > 0)
            {
                _pieces.Add(new Piece(BufferSource.Original, 0, TotalLength));
            }
        }
        /// <summary>
        /// Inserts text at the specified index by manipulating piece pointers.
        /// </summary>
        public void Insert(int index, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (index < 0 || index > TotalLength) throw new ArgumentOutOfRangeException(nameof(index));

            // 1. Append the new text to the Add Buffer
            int addBufferStartIndex = _addBuffer.Length;
            _addBuffer.Append(text);

            // Create the new piece that points to the text we just added
            Piece newPiece = new Piece(BufferSource.Add, addBufferStartIndex, text.Length);

            // 2. Handle insertion at the very end of the document (Fast path)
            if (index == TotalLength)
            {
                _pieces.Add(newPiece);
                TotalLength += text.Length;
                return;
            }

            // 3. Find which piece contains the 'index' where we want to insert
            int currentPosition = 0;
            for (int i = 0; i < _pieces.Count; i++)
            {
                Piece targetPiece = _pieces[i];

                if (currentPosition + targetPiece.Length > index)
                {
                    // We found the piece we need to split!
                    int offsetIntoPiece = index - currentPosition;

                    // Remove the old combined piece
                    _pieces.RemoveAt(i);

                    // If we are inserting exactly at the start of the piece, we don't need a left half.
                    if (offsetIntoPiece == 0)
                    {
                        _pieces.Insert(i, newPiece);
                        _pieces.Insert(i + 1, targetPiece);
                    }
                    else
                    {
                        // Split the piece into Left and Right halves
                        Piece leftHalf = new Piece(targetPiece.Source, targetPiece.Start, offsetIntoPiece);
                        Piece rightHalf = new Piece(targetPiece.Source, targetPiece.Start + offsetIntoPiece, targetPiece.Length - offsetIntoPiece);

                        // Insert them back into the list: Left -> New -> Right
                        _pieces.Insert(i, leftHalf);
                        _pieces.Insert(i + 1, newPiece);
                        _pieces.Insert(i + 2, rightHalf);
                    }

                    TotalLength += text.Length;
                    return;
                }

                currentPosition += targetPiece.Length;
            }
        }

        /// <summary>
        /// Reconstructs the full visible document for Win2D to render.
        /// </summary>
        public string GetText()
        {
            StringBuilder documentViewer = new StringBuilder(TotalLength);

            foreach (Piece piece in _pieces)
            {
                if (piece.Source == BufferSource.Original)
                {
                    documentViewer.Append(_originalBuffer.Substring(piece.Start, piece.Length));
                }
                else
                {
                    documentViewer.Append(_addBuffer.ToString(piece.Start, piece.Length));
                }
            }

            return documentViewer.ToString();
        }
    }
}