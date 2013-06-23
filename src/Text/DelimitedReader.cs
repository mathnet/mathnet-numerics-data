// <copyright file="DelimitedReader.cs" company="Math.NET">
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
using System.Reflection;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace MathNet.Numerics.Data.Text
{
    /// <summary>
    /// Creates a <see cref="Matrix{T}"/> from a delimited text file. If the user does not
    /// specify a delimiter, then any whitespace is used.
    /// </summary>
    /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
    public class DelimitedReader<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
    {
        // ReSharper disable StaticFieldInGenericType
        /// <summary>
        /// The base regular expression.
        /// </summary>
        private const string Base = "\\([^\\)]*\\)|'[^']*'|\"[^\"]*\"|[^{0}]*";

        /// <summary>
        /// The Type of of TDataType.
        /// </summary>
        private static readonly Type DataType = typeof (TDataType);

        /// <summary>
        /// Constructor to create dense matrices.
        /// </summary>
        private static readonly ConstructorInfo DenseConstructor;

        /// <summary>
        /// Constructor to create sparse matrices.
        /// </summary>
        private static readonly ConstructorInfo SparseConstructor;

        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Constructor to create matrix instance.
        /// </summary>
        private ConstructorInfo _constructor = DenseConstructor;

        /// <summary>
        /// The <see cref="CultureInfo"/> to use.
        /// </summary>
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

        /// <summary>
        /// A function that converts a string into the given data type.
        /// </summary>
        /// <returns>The converted number.</returns>
        private Func<string, object> _parseFunction;

        /// <summary>
        /// The regular expression to use.
        /// </summary>
        private Regex _regex = new Regex(string.Format(Base, @"\s"), RegexOptions.Compiled);

        /// <summary>
        /// Sets the Type for the Dense and Sparse matrices.
        /// </summary>
        static DelimitedReader()
        {
            if (DataType == typeof (double))
            {
                DenseConstructor = typeof (DenseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
                SparseConstructor = typeof (SparseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
            }
            else if (DataType == typeof (float))
            {
                DenseConstructor =
                    typeof (LinearAlgebra.Single.DenseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
                SparseConstructor =
                    typeof (LinearAlgebra.Single.SparseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
            }
            else if (DataType == typeof (Complex))
            {
                DenseConstructor =
                    typeof (LinearAlgebra.Complex.DenseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
                SparseConstructor =
                    typeof (LinearAlgebra.Complex.SparseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
            }
            else if (DataType == typeof (Complex32))
            {
                DenseConstructor =
                    typeof (LinearAlgebra.Complex32.DenseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
                SparseConstructor =
                    typeof (LinearAlgebra.Complex32.SparseMatrix).GetConstructor(new[] {typeof (int), typeof (int)});
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TDataType}"/> class using
        /// any whitespace as the delimiter and generating dense matrices.
        /// </summary>
        public DelimitedReader()
        {
            SetParser();
        }

        /// <summary>
        /// Whether to create sparse matrices or not. Defaults to false.
        /// </summary>
        public bool Sparse
        {
            set
            {
                if (value)
                {
                    _constructor = SparseConstructor;
                }
            }
        }

        /// <summary>
        /// The delimiter to use for parsing. Defaults to any whitespace.
        /// </summary>
        public string Delmiter
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _regex = new Regex(string.Format(Base, value), RegexOptions.Compiled);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to use when parsing the numbers.
        /// </summary>
        /// <value>The culture info.</value>
        /// <remarks>Defaults to <c>CultureInfo.CurrentCulture</c>.</remarks>
        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }

            set
            {
                if (value != null)
                {
                    _cultureInfo = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the files has a header row.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has a header row; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Defaults to <see langword="false"/>.</remarks>
        public bool HasHeaderRow { get; set; }

        /// <summary>
        /// Sets the parse function for the given TDataType.
        /// Note: This cannot be made static because of _cultureInfo.
        /// </summary>
        private void SetParser()
        {
            if (DataType == typeof (double))
            {
                _parseFunction = number => double.Parse(number, NumberStyles.Any, _cultureInfo);
            }
            else if (DataType == typeof (float))
            {
                _parseFunction = number => float.Parse(number, NumberStyles.Any, _cultureInfo);
            }
            else if (DataType == typeof (Complex))
            {
                _parseFunction = number => number.ToComplex(_cultureInfo);
            }
            else if (DataType == typeof (Complex32))
            {
                _parseFunction = number => number.ToComplex32(_cultureInfo);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Performs the actual reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the matrix from.</param>
        /// <returns>
        /// A matrix containing the data from the <see cref="Stream"/>. <see langword="null"/> is returned if the <see cref="Stream"/> is empty.
        /// </returns>
        public Matrix<TDataType> ReadMatrix(Stream stream)
        {
            var data = new List<string[]>();

            // max is used to supports files like:
            // 1,2
            // 3,4,5,6
            // 7
            // this creates a 3x4 matrix:
            // 1, 2, 0 ,0 
            // 3, 4, 5, 6
            // 7, 0, 0, 0
            var max = -1;

            var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            if (HasHeaderRow)
            {
                line = reader.ReadLine();
            }

            while (line != null)
            {
                line = line.Trim();
                if (line.Length > 0)
                {
                    var matches = _regex.Matches(line);
                    var row = (from Match match in matches where match.Length > 0 select match.Value).ToArray();
                    max = Math.Max(max, row.Length);
                    data.Add(row);
                }

                line = reader.ReadLine();
            }

            var ret = (Matrix<TDataType>) _constructor.Invoke(new object[] {data.Count, max});

            if (data.Count != 0)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    for (var j = 0; j < row.Length; j++)
                    {
                        // strip off quotes
                        var value = row[j].Replace("'", string.Empty).Replace("\"", string.Empty);
                        ret[i, j] = (TDataType) _parseFunction(value);
                    }
                }
            }

            reader.Close();
            reader.Dispose();
            return ret;
        }

        /// <summary>
        /// Creates a DelimtedReader that returns matrices of type TMatrixTye.
        /// </summary>
        /// <typeparam name="TMatrixType">The type of matrix to return.</typeparam>
        /// <returns>A delimited reader</returns>
        public static DelimitedReader<TDataType> OfMatrixType<TMatrixType>() where TMatrixType : Matrix<TDataType>
        {
            var reader = new DelimitedReader<TDataType>
                {
                    _constructor = typeof (TMatrixType).GetConstructor(new[] {typeof (int), typeof (int)})
                };
            return reader;
        }
    }
}