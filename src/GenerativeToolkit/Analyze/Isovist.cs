﻿using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using Dynamo.Graph.Nodes;
using GenerativeToolkit.Graphs;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using DSPoint = Autodesk.DesignScript.Geometry.Point;

namespace Autodesk.GenerativeToolkit.Analyze
{
    public static class Isovist
    {
        /// <summary>
        /// Returns a surface representing the Isovist area visible from 
        /// the given point.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="internals">List of Polygons representing internal obstructions</param>
        /// <param name="point">Origin point</param>
        /// <returns name="isovist">Surface representing the isovist area</returns>
        [NodeCategory("Actions")]
        public static Surface FromPoint(
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> internals,
            DSPoint point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);

            if (baseGraph == null) throw new ArgumentNullException("graph");
            if (point == null) throw new ArgumentNullException("point");

            GeometryVertex origin = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GeometryVertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<DSPoint> points = vertices.Select(v => Points.ToPoint(v)).ToList();

            var polygon = Polygon.ByPoints(points);

            // if polygon is self intersecting, make new polygon
            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);

            }
            Surface surface = Surface.ByPatch(polygon);
            polygon.Dispose();
            points.ForEach(p => p.Dispose());

            return surface;
        }
    }
}