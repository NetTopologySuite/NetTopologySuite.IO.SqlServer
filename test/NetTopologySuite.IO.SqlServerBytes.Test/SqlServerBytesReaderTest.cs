﻿using System;
using System.Globalization;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.Properties;
using Xunit;

namespace NetTopologySuite.IO
{
    public class SqlServerBytesReaderTest
    {
        [Theory]
        [InlineData(
            "POINT EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF01")]
        [InlineData(
            "POINT (1 2)",
            "00000000010C000000000000F03F0000000000000040")]
        [InlineData(
            "POINT Z(1 2 3)",
            "00000000010D000000000000F03F00000000000000400000000000000840")]
        [InlineData(
            "LINESTRING EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF02")]
        [InlineData(
            "LINESTRING (0 0, 0 1)",
            "000000000114000000000000000000000000000000000000000000000000000000000000F03F")]
        [InlineData(
            "LINESTRING Z(0 0 1, 0 1 2)",
            "000000000115000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F0000000000000040")]
        [InlineData(
            "LINESTRING Z(0 0 NaN, 0 1 2)",
            "000000000115000000000000000000000000000000000000000000000000000000000000F03F000000000000F8FF0000000000000040")]
        [InlineData(
            "LINESTRING (0 0, 0 1, 0 2)",
            "00000000010403000000000000000000000000000000000000000000000000000000000000000000F03F0000000000000000000000000000004001000000010000000001000000FFFFFFFF0000000002")]
        [InlineData(
            "LINESTRING Z(0 0 1, 0 1 2, 0 2 3)",
            "00000000010503000000000000000000000000000000000000000000000000000000000000000000F03F00000000000000000000000000000040000000000000F03F0000000000000040000000000000084001000000010000000001000000FFFFFFFF0000000002")]
        [InlineData(
            "POLYGON EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF03")]
        [InlineData(
            "POLYGON ((0 0, 0 1, 1 1, 0 0))",
            "00000000010404000000000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000000000000000000000001000000020000000001000000FFFFFFFF0000000003")]
        [InlineData(
            "POLYGON ((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1))",
            "0000000001040A0000000000000000000000000000000000000000000000000000000000000000000840000000000000084000000000000008400000000000000840000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000040000000000000004000000000000000400000000000000040000000000000F03F000000000000F03F000000000000F03F020000000200000000000500000001000000FFFFFFFF0000000003")]
        [InlineData(
            "GEOMETRYCOLLECTION EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF07")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0))",
            "000000000104010000000000000000000000000000000000000001000000010000000002000000FFFFFFFF0000000007000000000000000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0), POINT (0 1))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F020000000100000000010100000003000000FFFFFFFF0000000007000000000000000001000000000100000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (0 1)))",
            "000000000104010000000000000000000000000000000000F03F01000000010000000003000000FFFFFFFF0000000007000000000000000007010000000000000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0), GEOMETRYCOLLECTION (POINT (0 1)))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F020000000100000000010100000004000000FFFFFFFF0000000007000000000000000001000000000100000007020000000100000001")]
        [InlineData(
            "MULTIPOINT ((0 0))",
            "000000000104010000000000000000000000000000000000000001000000010000000002000000FFFFFFFF0000000004000000000000000001")]
        [InlineData(
            "MULTILINESTRING ((0 0, 0 1))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F01000000010000000002000000FFFFFFFF0000000005000000000000000002")]
        [InlineData(
            "MULTIPOLYGON (((0 0, 0 1, 1 1, 0 0)))",
            "00000000010404000000000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000000000000000000000001000000020000000002000000FFFFFFFF0000000006000000000000000003")]
        [InlineData(
            "CIRCULARSTRING (0 0, 1 1, 2 0)",
            "0000000002040300000000000000000000000000000000000000000000000000F03F000000000000F03F0000000000000040000000000000000001000000020000000001000000FFFFFFFF0000000008")]
        // TODO: Test CompoundCurve with consecutive lines/arcs
        [InlineData(
            "COMPOUNDCURVE ((0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))",
            "0000000002040400000000000000000000000000000000000000000000000000F03F00000000000000000000000000000040000000000000F03F0000000000000840000000000000000001000000030000000001000000FFFFFFFF0000000009020000000203")]
        // TODO: Test CurvePolygon with interior rings
        [InlineData(
            "CURVEPOLYGON (CIRCULARSTRING (2 1, 1 2, 0 1, 1 0, 2 1))",
            "000000000204050000000000000000000040000000000000F03F000000000000F03F00000000000000400000000000000000000000000000F03F000000000000F03F00000000000000000000000000000040000000000000F03F01000000020000000001000000FFFFFFFF000000000A")]
        public void Read_works(string expected, string bytes)
        {
            Assert.Equal(expected, Read(bytes).AsText());
        }

        [Theory]
        [InlineData(
            "POINT (1 2)",
            "E6100000010C0000000000000040000000000000F03F")]
        [InlineData(
            "POLYGON ((0 0, 3 0, 3 3, 0 3, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1))",
            "E610000001040A000000000000000000F03F000000000000F03F0000000000000040000000000000F03F00000000000000400000000000000040000000000000F03F0000000000000040000000000000F03F000000000000F03F0000000000000000000000000000000000000000000000000000000000000840000000000000084000000000000008400000000000000840000000000000000000000000000000000000000000000000020000000200000000000500000001000000FFFFFFFF0000000003")]
        public void Read_works_when_IsGeography(string expected, string bytes)
        {
            Assert.Equal(expected, Read(bytes, isGeography: true).AsText());
        }

        [Theory]
        [InlineData(
            "E610000002240400000000000000000000000000000000000000000000000000F03F0000000000000000000000000000F03F000000000000F03F0000000000000000000000000000000001000000010000000001000000FFFFFFFF0000000003")]
        [InlineData(
            "E610000002240800000000000000000000000000000000000000000000000000F03F0000000000000000000000000000F03F000000000000F03F000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F0000000000000040000000000000000000000000000000400000000000000000000000000000F03F020000000100000000010400000001000000FFFFFFFF0000000003")]
        public void Read_throws_when_IsGeography_and_no_shell(string bytes)
        {
            var ex = Assert.Throws<ArgumentException>(
                () => Read(bytes, isGeography: true));

            Assert.Equal("shell is empty but holes are not", ex.Message);
        }

        [Fact]
        public void Read_works_with_SRID()
        {
            var point = Read("E6100000010C00000000000000000000000000000000");

            Assert.Equal(4326, point.SRID);
            Assert.Equal("POINT (0 0)", point.AsText());
        }

        [Fact]
        public void Read_works_when_Point_with_M()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var point = (Point)Read(
                "00000000010F000000000000F03F000000000000004000000000000008400000000000001040",
                geometryServices);

            Assert.Equal("POINT ZM(1 2 3 4)", new WKTWriter(4).Write(point));
        }

        [Fact]
        public void Read_works_when_LineString_with_Ms()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var lineString = (LineString)Read(
                "000000000117000000000000000000000000000000000000000000000000000000000000F03F00000000000000000000000000000000000000000000F03F0000000000000040",
                geometryServices);

            Assert.Equal("LINESTRING ZM(0 0 0 1, 0 1 0 2)", new WKTWriter(4).Write(lineString));
        }

        [Fact]
        public void Read_works_when_null()
        {
            Assert.Null(Read("FFFFFFFF"));
        }

        [Fact]
        public void HandleOrdinates_works()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var point = (Point)Read(
                "00000000010F000000000000F03F000000000000004000000000000008400000000000001040",
                geometryServices,
                Ordinates.XY);

            Assert.Equal("POINT (1 2)", new WKTWriter(4).Write(point));
        }

        [Fact]
        public void Read_throws_when_full_globe()
        {
            var ex = Assert.Throws<ParseException>(
                () => Read("E61000000224000000000000000001000000FFFFFFFFFFFFFFFF0B"));

            Assert.Equal(string.Format(Resources.UnexpectedGeographyType, "FullGlobe"), ex.Message);
        }

        private Geometry Read(
            string bytes,
            NtsGeometryServices geometryServices = null,
            Ordinates handleOrdinates = Ordinates.XYZM,
            bool isGeography = false)
        {
            byte[] byteArray = new byte[bytes.Length / 2];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                byteArray[i / 2] = byte.Parse(bytes.Substring(i, 2), NumberStyles.HexNumber);
            }

            var reader = new SqlServerBytesReader(geometryServices ?? NtsGeometryServices.Instance)
            {
                HandleOrdinates = handleOrdinates,
                IsGeography = isGeography
            };

            return reader.Read(byteArray);
        }
    }
}
