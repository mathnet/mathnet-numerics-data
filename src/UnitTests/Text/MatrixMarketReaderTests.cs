// <copyright file="MatrixMarketReaderTests.cs" company="Math.NET">
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

using System.Numerics;
using MathNet.Numerics.Data.Text;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Text
{
    [TestFixture]
    public class MatrixMarketReaderTests
    {
        [Test]
        public void CanReadFudao007AsDouble()
        {
            var m = MatrixMarketReader.ReadMatrix<double>("./data/MatrixMarket/fidap007.mtx");
            Assert.IsInstanceOf<LinearAlgebra.Double.SparseMatrix>(m);
            Assert.AreEqual(1633, m.RowCount);
            Assert.AreEqual(1633, m.ColumnCount);
            Assert.GreaterOrEqual(54487, ((LinearAlgebra.Double.SparseMatrix) m).NonZerosCount);
            Assert.Less(46000, ((LinearAlgebra.Double.SparseMatrix) m).NonZerosCount);
            Assert.AreEqual(-6.8596032449032e+06d, m[1604, 1631]);
            Assert.AreEqual(-9.1914585107976e+06d, m[1616, 1628]);
            Assert.AreEqual(7.9403870156486e+07d, m[905, 726]);
        }

        [Test]
        public void CanReadFudao007AsSingle()
        {
            var m = MatrixMarketReader.ReadMatrix<float>("./data/MatrixMarket/fidap007.mtx");
            Assert.IsInstanceOf<LinearAlgebra.Single.SparseMatrix>(m);
            Assert.AreEqual(1633, m.RowCount);
            Assert.AreEqual(1633, m.ColumnCount);
            Assert.GreaterOrEqual(54487, ((LinearAlgebra.Single.SparseMatrix)m).NonZerosCount);
            Assert.Less(46000, ((LinearAlgebra.Single.SparseMatrix)m).NonZerosCount);
            Assert.AreEqual(-6.8596032449032e+06f, m[1604, 1631]);
            Assert.AreEqual(-9.1914585107976e+06f, m[1616, 1628]);
            Assert.AreEqual(7.9403870156486e+07f, m[905, 726]);
        }

        [Test]
        public void CanReadFudao007AsComplex()
        {
            var m = MatrixMarketReader.ReadMatrix<Complex>("./data/MatrixMarket/fidap007.mtx");
            Assert.IsInstanceOf<LinearAlgebra.Complex.SparseMatrix>(m);
            Assert.AreEqual(1633, m.RowCount);
            Assert.AreEqual(1633, m.ColumnCount);
            Assert.GreaterOrEqual(54487, ((LinearAlgebra.Complex.SparseMatrix)m).NonZerosCount);
            Assert.Less(46000, ((LinearAlgebra.Complex.SparseMatrix)m).NonZerosCount);
            Assert.AreEqual(-6.8596032449032e+06d, m[1604, 1631].Real);
            Assert.AreEqual(0.0d, m[1604, 1631].Imaginary);
            Assert.AreEqual(-9.1914585107976e+06d, m[1616, 1628].Real);
            Assert.AreEqual(0.0d, m[1616, 1628].Imaginary);
            Assert.AreEqual(7.9403870156486e+07d, m[905, 726].Real);
            Assert.AreEqual(0.0d, m[905, 726].Imaginary);
        }

        [Test]
        public void CanReadFudao007AsComplex32()
        {
            var m = MatrixMarketReader.ReadMatrix<Complex32>("./data/MatrixMarket/fidap007.mtx");
            Assert.IsInstanceOf<LinearAlgebra.Complex32.SparseMatrix>(m);
            Assert.AreEqual(1633, m.RowCount);
            Assert.AreEqual(1633, m.ColumnCount);
            Assert.GreaterOrEqual(54487, ((LinearAlgebra.Complex32.SparseMatrix) m).NonZerosCount);
            Assert.Less(46000, ((LinearAlgebra.Complex32.SparseMatrix) m).NonZerosCount);
            Assert.AreEqual(-6.8596032449032e+06f, m[1604, 1631].Real);
            Assert.AreEqual(0.0f, m[1604, 1631].Imaginary);
            Assert.AreEqual(-9.1914585107976e+06f, m[1616, 1628].Real);
            Assert.AreEqual(0.0f, m[1616, 1628].Imaginary);
            Assert.AreEqual(7.9403870156486e+07f, m[905, 726].Real);
            Assert.AreEqual(0.0f, m[905, 726].Imaginary);
        }
    }
}
