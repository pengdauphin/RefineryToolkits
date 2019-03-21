﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    internal class BuildingD : BuildingBase
    {
        private double facetLength;

        public BuildingD()
        {
            Type = ShapeType.D;
        }

        protected override void Setup()
        {
            if (!IsCurved)
            {
                facetLength = Width / (1 + Math.Sqrt(2));
            }

            UsesDepth = Width <= Depth * 2 || Length <= Depth * 2 ? false : true;
        }

        protected override (Curve boundary, List<Curve> holes) CreateBaseCurves()
        {
            // The D is pointing "down" so that it matches with the U.

            var holes = new List<Curve>();
            var boundaryCurves = new List<Curve>();

            var arcHeight = Math.Min(
                Length - (UsesDepth ? Depth : 0),
                Width / 2);

            Plane arcCenter = Plane.ByOriginNormal(
                Point.ByCoordinates(Width / 2, arcHeight),
                Vector.ZAxis());

            if (IsCurved)
            {
                boundaryCurves.Add(EllipseArc.ByPlaneRadiiAngles(arcCenter, Width / 2, arcHeight, 180, 180));

                if (arcHeight < Length)
                {
                    // Outside of D has a square back.
                    boundaryCurves.Add(PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(Width, arcHeight),
                            Point.ByCoordinates(Width, Length),
                            Point.ByCoordinates(0, Length),
                            Point.ByCoordinates(0, arcHeight)
                        }));
                }
                else
                {
                    // Outside of D is half an ellipse (or circle).
                    boundaryCurves.Add(Line.ByStartPointEndPoint(
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length)));
                }

                if (UsesDepth)
                {
                    var curves = new List<Curve>
                    {
                        EllipseArc.ByPlaneRadiiAngles(arcCenter, (Width / 2) - Depth, arcHeight - Depth, 0, -180)
                    };

                    if (arcHeight < Length - Depth)
                    {
                        curves.Add(PolyCurve.ByPoints(new[]
                        {
                            Point.ByCoordinates(Depth, arcHeight),
                            Point.ByCoordinates(Depth, Length - Depth),
                            Point.ByCoordinates(Width - Depth, Length - Depth),
                            Point.ByCoordinates(Width - Depth, arcHeight)
                        }));
                    }
                    else
                    {
                        curves.Add(Line.ByStartPointEndPoint(
                            Point.ByCoordinates(Depth, arcHeight),
                            Point.ByCoordinates(Width - Depth, arcHeight)));
                    }

                    holes.Add(PolyCurve.ByJoinedCurves(curves));

                    curves.ForEach(x => x.Dispose());
                }
            }
            else
            {
                // Faceted D.

                double baseWidth = Width * Math.Tan(Math.PI / 8);
                double sideWidth = baseWidth * arcHeight / (2 * Width);

                boundaryCurves.Add(PolyCurve.ByPoints(new[]
                    {
                        Point.ByCoordinates(Width, Length),
                        Point.ByCoordinates(0, Length),
                        Point.ByCoordinates(0, arcHeight - sideWidth),
                        Point.ByCoordinates((Width - baseWidth) / 2, 0),
                        Point.ByCoordinates((Width + baseWidth) / 2, 0),
                        Point.ByCoordinates(Width, arcHeight - sideWidth)
                    }, 
                    connectLastToFirst: true));
                
                if (UsesDepth)
                {
                    double angleA = Math.Atan2(2 * (arcHeight - sideWidth), Width - baseWidth);
                    double offsetBaseWidth = baseWidth - (2 * Depth / Math.Tan((Math.PI - angleA) / 2));
                    double offsetSideWidth = sideWidth - (Depth / Math.Tan((angleA / 2) + (Math.PI / 4)));

                    holes.Add(PolyCurve.ByPoints(new []
                        {
                            Point.ByCoordinates(Width - Depth, Length - Depth),
                            Point.ByCoordinates(Width - Depth, arcHeight - offsetSideWidth),
                            Point.ByCoordinates((Width + offsetBaseWidth) / 2, Depth),
                            Point.ByCoordinates((Width - offsetBaseWidth) / 2, Depth),
                            Point.ByCoordinates(Depth, arcHeight - offsetSideWidth),
                            Point.ByCoordinates(Depth, Length - Depth)

                        },
                        connectLastToFirst: true));
                }

            }

            var boundary = PolyCurve.ByJoinedCurves(boundaryCurves);

            return (boundary, holes);
        }

        protected override List<Curve> CreateCoreCurves()
        {
            if (UsesDepth)
            {
                // One core along the top leg of building.

                double coreHeight = Depth * (1 - (2 * hallwayToDepth));

                return new List<Curve>
                {
                    Rectangle.ByWidthLength(
                        Plane.ByOriginNormal(Point.ByCoordinates(Width / 2, Length - (Depth / 2)), Vector.ZAxis()),
                        CoreArea / coreHeight,
                        coreHeight)
                };
            }
            else
            {
                // Simple box building, core is in the center with the same aspect ratio as floorplate.
                return base.CreateCoreCurves();
            }
        }
    }
}
