﻿namespace Microsoft.Formula.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.Serialization;
    using Common;

    public struct Span
    {
        public static Span Unknown
        {
            get { return default(Span); }
        }

        private int startLine;
        private int startCol;
        private int endLine;
        private int endCol;

        public int StartLine
        {
            get { return startLine; }
        }

        public int StartCol
        {
            get { return startCol; }
        }

        public int EndLine
        {
            get { return endLine; }
        }

        public int EndCol
        {
            get { return endCol; }
        }

        public Span(int startLine, int startCol, int endLine, int endCol)
        {
            this.startLine = startLine;
            this.startCol = startCol;
            this.endLine = endLine;
            this.endCol = endCol;
        }

        public static int Compare(Span s, Span t)
        {
            if (s.Equals(t))
            {
                return 0;
            }
            else if (s.startLine != t.startLine)
            {
                return s.startLine - t.startLine;
            }
            else if (s.startCol != t.startCol)
            {
                return s.startCol - t.startCol;
            }
            else if (s.endLine != t.endLine)
            {
                return s.endLine - t.endLine;
            }
            else if (s.endCol != t.endCol)
            {
                return s.endCol - t.endCol;
            }

            throw new Impossible();
        }

    }
}
