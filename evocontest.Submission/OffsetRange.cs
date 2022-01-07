using System;
using System.Diagnostics;

namespace evocontest.Submission
{
    [DebuggerDisplay("{ToDebug()}")]
    public struct OffsetRange
    {
        public readonly Index start;
        public readonly Index end;
        public int offset;

        public int Length => end.Value - start.Value;

        public static implicit operator OffsetRange(Range range) => new OffsetRange(range.Start, range.End, 0);
        public static implicit operator Range(OffsetRange offsetRange) => new Range(offsetRange.start.Value + offsetRange.offset, offsetRange.end.Value + offsetRange.offset);
        public static implicit operator int(OffsetRange range) => range.start.Value + range.offset;
        public static implicit operator OffsetRange(int offset) => offset..(offset + 1);

        public Range AsRange() => this;

        public static string source = string.Empty;
        public string ToDebug() => source.Length >= offset + end.Value ? source[AsRange()] : ToString();

        public override string ToString() => AsRange().ToString();

        public override bool Equals(object obj)
        {
            if (!(obj is OffsetRange other)) return false;
            if (this.Length != other.Length) return false;
            if (source == string.Empty) return false;
            for (int i = 0; i < Length; i++)
            {
                if (source[i + this.start.Value + this.offset] != source[i + other.start.Value + other.offset]) return false;
            }
            return true;
        }

        private OffsetRange(Index s, Index e, int o)
        {
            start = s;
            end = e;
            offset = o;
        }
    }
}
