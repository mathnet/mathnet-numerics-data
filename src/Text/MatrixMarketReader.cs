// <copyright file="MatrixMarketReader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Data.Text
{
    /// <summary>
    /// NIST MatrixMarket Format Reader (http://math.nist.gov/MatrixMarket/)
    /// </summary>
    public static class MatrixMarketReader
    {
        static readonly char[] Separators = {' '};
        static readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public static Matrix<T> ReadMatrix<T>(string filePath) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = File.OpenRead(filePath))
            {
                return ReadMatrix<T>(reader);
            }
        }

        public static Vector<T> ReadVector<T>(string filePath) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = File.OpenRead(filePath))
            {
                return ReadVector<T>(reader);
            }
        }

        public static Matrix<T> ReadMatrix<T>(Stream stream) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = new StreamReader(stream))
            {
                return ReadMatrix<T>(reader);
            }
        }

        public static Vector<T> ReadVector<T>(Stream stream) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = new StreamReader(stream))
            {
                return ReadVector<T>(reader);
            }
        }

        public static Matrix<T> ReadMatrix<T>(TextReader reader) where T : struct, IEquatable<T>, IFormattable
        {
            bool complex, sparse;
            ExpectHeader(reader, true, out complex, out sparse);

            var parse = CreateValueParser<T>(complex);

            var sizes = ExpectLine(reader).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            int rows = int.Parse(sizes[0]);
            int cols = int.Parse(sizes[1]);

            if (sparse)
            {
                var indexedSeq = ReadLines(reader).Select(line =>
                {
                    string[] vals = line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<int, int, T>(int.Parse(vals[0]) - 1, int.Parse(vals[1]) - 1, parse(2, vals));
                });
                return Matrix<T>.Build.SparseMatrixOfIndexed(rows, cols, indexedSeq);
            }
            else
            {
                var columnMajorSeq = ReadLines(reader).Select(line => parse(0, line.Split(Separators, StringSplitOptions.RemoveEmptyEntries)));
                return Matrix<T>.Build.DenseMatrixOfColumnMajor(rows, cols, columnMajorSeq);
            }
        }

        public static Vector<T> ReadVector<T>(TextReader reader)
            where T : struct, IEquatable<T>, IFormattable
        {
            bool complex, sparse;
            ExpectHeader(reader, false, out complex, out sparse);

            var parse = CreateValueParser<T>(complex);

            var sizes = ExpectLine(reader).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            int length = int.Parse(sizes[0]);

            if (sparse)
            {
                var indexedSeq = ReadLines(reader).Select(line =>
                {
                    string[] vals = line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<int, T>(int.Parse(vals[0]) - 1, parse(1, vals));
                });
                return Vector<T>.Builder.SparseVectorOfIndexedEnumerable(length, indexedSeq);

            }
            else
            {
                var values = ReadLines(reader).Select(line => parse(0, line.Split(Separators, StringSplitOptions.RemoveEmptyEntries)));
                return Vector<T>.Builder.DenseVector(values.ToArray());
            }
        }

        static void ExpectHeader(TextReader reader, bool matrix, out bool complex, out bool sparse)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("%%MatrixMarket"))
                {
                    var tokens = line.ToLowerInvariant().Substring(15).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length != 4)
                    {
                        throw new FormatException(@"Expected MatrixMarket Header with 4 attributes: object, format, field, symmetry; see http://math.nist.gov/MatrixMarket/ for details.");
                    }
                    if (tokens[0] != (matrix ? "matrix" : "vector"))
                    {
                        throw new FormatException("Expected matrix content.");
                    }
                    if (tokens[3] != "general") // general | symmetric | skew-symmetric | hermitian
                    {
                        throw new FormatException("Expected matrix to be in general format.");
                    }

                    switch (tokens[2])
                    {
                        case "real":
                        case "double":
                        case "integer":
                            complex = false;
                            break;
                        case "complex":
                            complex = true;
                            break;
                        default:
                            throw new NotSupportedException("Field type not supported.");
                    }

                    switch (tokens[1])
                    {
                        case "array":
                            sparse = false;
                            break;
                        case "coordinate":
                            sparse = true;
                            break;
                        default:
                            throw new NotSupportedException("Format type not supported.");
                    }

                    return;
                }
            }

            throw new FormatException(@"Expected MatrixMarket Header, see http://math.nist.gov/MatrixMarket/ for details.");
        }

        static string ExpectLine(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trim = line.Trim();
                if (trim.Length > 0 && !trim.StartsWith("%"))
                {
                    return trim;
                }
            }

            throw new FormatException(@"End of file reached unexpectedly.");
        }

        static IEnumerable<string> ReadLines(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trim = line.Trim();
                if (trim.Length > 0 && !trim.StartsWith("%"))
                {
                    yield return trim;
                }
            }
        }

        static Func<int, string[], T> CreateValueParser<T>(bool sourceIsComplex)
        {
            if (typeof (T) == typeof (double))
            {
                // ignore imaginary part if source is complex
                return (offset, tokens) => (T) (object) double.Parse(tokens[offset], NumberStyles.Any, Format);
            }
            if (typeof (T) == typeof (float))
            {
                // ignore imaginary part if source is complex
                return (offset, tokens) => (T) (object) float.Parse(tokens[offset], NumberStyles.Any, Format);
            }
            if (typeof (T) == typeof (Complex))
            {
                return sourceIsComplex
                    ? (Func<int, string[], T>) ((offset, tokens) => (T) (object) new Complex(double.Parse(tokens[offset], NumberStyles.Any, Format), double.Parse(tokens[offset + 1], NumberStyles.Any, Format)))
                    : ((offset, tokens) => (T) (object) new Complex(double.Parse(tokens[offset], NumberStyles.Any, Format), 0d));
            }
            if (typeof (T) == typeof (Complex32))
            {
                return sourceIsComplex
                    ? (Func<int, string[], T>) ((offset, tokens) => (T) (object) new Complex32(float.Parse(tokens[offset], NumberStyles.Any, Format), float.Parse(tokens[offset + 1], NumberStyles.Any, Format)))
                    : ((offset, tokens) => (T) (object) new Complex32(float.Parse(tokens[offset], NumberStyles.Any, Format), 0f));
            }
            throw new NotSupportedException();
        }
    }
}
