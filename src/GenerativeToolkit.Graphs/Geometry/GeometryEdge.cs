﻿#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Extensions;
#endregion


namespace GenerativeToolkit.Graphs.Geometry
{
    /// <summary>
    /// Representation of Edges on a graph
    /// </summary>
    public class GeometryEdge : GeometryBase
    {
        #region Variables
        /// <summary>
        /// StartVertex
        /// </summary>
        public GeometryVertex StartVertex { get; private set; }

        /// <summary>
        /// EndVertex
        /// </summary>
        public GeometryVertex EndVertex { get; private set; }


        public double Length { get; private set; }

        public GeometryVector Direction { get; private set; }

        #endregion

        #region Constructors
        public GeometryEdge(GeometryVertex start, GeometryVertex end)
        {
            StartVertex = start;
            EndVertex = end;
            Length = StartVertex.DistanceTo(EndVertex);
            Direction = GeometryVector.ByTwoVertices(StartVertex, EndVertex);
        }

        /// <summary>
        /// gEdge constructor by start and end vertices
        /// </summary>
        /// <param name="start">Start vertex</param>
        /// <param name="end">End gVertex</param>
        /// <returns name="edge">edge</returns>
        public static GeometryEdge ByStartVertexEndVertex(GeometryVertex start, GeometryVertex end)
        {
            return new GeometryEdge(start, end);
        }

        /// <summary>
        /// gEdge constructor by line
        /// </summary>
        /// <param name="line">line</param>
        /// <returns name="edge">edge</returns>
        //public static gEdge ByLine(Line line)
        //{
        //    gVertex start = gVertex.ByCoordinates(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
        //    gVertex end = gVertex.ByCoordinates(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
        //    return new gEdge(start, end);
        //}
        #endregion

        /// <summary>
        /// Method to check if vertex belongs to edge
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Contains(GeometryVertex vertex)
        {
            return StartVertex.Equals(vertex) || EndVertex.Equals(vertex);
        }

        /// <summary>
        /// Method to return the other end vertex of the gEdge
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public GeometryVertex GetVertexPair(GeometryVertex vertex)
        {
            return (StartVertex.Equals(vertex)) ? EndVertex : StartVertex;
        }

        public bool IsCoplanarTo(GeometryEdge edge)
        {
            // http://mathworld.wolfram.com/Coplanar.html
            GeometryVector a = this.Direction;
            GeometryVector b = edge.Direction;
            GeometryVector c = GeometryVector.ByTwoVertices(this.StartVertex, edge.StartVertex);

            return c.Dot(a.Cross(b)) == 0;
        }

        public GeometryBase Intersection(GeometryEdge other)
        {
            // http://mathworld.wolfram.com/Line-LineIntersection.html
            if (!this.BoundingBox.Intersects(other.BoundingBox)) { return null; }
            if (!this.IsCoplanarTo(other)) { return null; }
            if (this.Equals(other)) { return this; } // Issues if same polygon id???

            var a = this.Direction;
            var b = other.Direction;
            
            if (a.IsParallelTo(b))
            {
                // Fully contains the test edge
                if (other.StartVertex.OnEdge(this) && other.EndVertex.OnEdge(this)) { return other; }
                // Is fully contained by test edge
                else if (this.StartVertex.OnEdge(other) && this.EndVertex.OnEdge(other)) { return this; }
                // Not fully inclusive but overlapping
                else if (this.StartVertex.OnEdge(other) || this.EndVertex.OnEdge(other))
                {
                    GeometryVertex[] vertices = new GeometryVertex[4]
                    {
                        this.StartVertex,
                        this.EndVertex,
                        other.StartVertex,
                        other.EndVertex
                    };
                    var sorted = vertices.OrderBy(v => v.Y).ThenBy(v => v.X).ThenBy(v => v.Z).ToList();
                    return GeometryEdge.ByStartVertexEndVertex(sorted[1], sorted[2]);
                }
                // Not intersecting
                else
                {
                    return null;
                }
            }

            // No parallels but intersecting on one of the extreme vertices
            if (other.Contains(this.StartVertex)) { return this.StartVertex; }
            else if (other.Contains(this.EndVertex)) { return this.EndVertex; }


            // No coincident nor same extremes
            var c = GeometryVector.ByTwoVertices(this.StartVertex, other.StartVertex);
            var cxb = c.Cross(b);
            var axb = a.Cross(b);
            var dot = cxb.Dot(axb);

            // If dot == 0 it means that other edge contains at least a vertex from this edge
            // and they are parallel or perpendicular. Cannot be parallel as that was tested before.
            // It might also mean they don't intersect but the would if extending the projections
            double s = (dot) / Math.Pow(axb.Length, 2);

            if (s.AlmostEqualTo(0))
            {
                if (this.StartVertex.OnEdge(other)) { return this.StartVertex; }
                else if(this.EndVertex.OnEdge(other)) { return this.EndVertex; }
                else if(other.StartVertex.OnEdge(this)) { return other.StartVertex; }
                else if(other.EndVertex.OnEdge(this)) { return other.EndVertex; }
                else { return null; }
            }

            
            
            // s > 1, means that "intersection" vertex is not on either edge
            // s == NaN means they are parallels so never intersect
            if (s < 0 || s > 1 || Double.IsNaN(s)) { return null; }

            GeometryVertex intersection = this.StartVertex.Translate(a.Scale(s));

            if (intersection.Equals(other.StartVertex)){ return other.StartVertex; }
            if (intersection.Equals(other.EndVertex)) { return other.EndVertex; }
            if (!intersection.OnEdge(other))
            {
                return null;
            }

            return intersection;
        }

        public bool Intersects(GeometryEdge edge)
        {
            if(this.StartVertex.OnEdge(edge) || this.EndVertex.OnEdge(edge))
            {
                if (this.Direction.IsParallelTo(edge.Direction))
                {
                    return true;
                }
            }
            return this.Intersection(edge) != null;
        }

        public double DistanceTo(GeometryVertex vertex)
        {
            return vertex.DistanceTo(this);
        }

        public double DistanceTo(GeometryEdge edge)
        {
            // http://mathworld.wolfram.com/Line-LineDistance.html
            if (this.IsCoplanarTo(edge))
            {
                var distances = new double[4]{
                    StartVertex.DistanceTo(edge),
                    EndVertex.DistanceTo(edge),
                    edge.StartVertex.DistanceTo(this),
                    edge.EndVertex.DistanceTo(this)
                };
                return distances.Min();
            }else
            {
                var a = this.Direction;
                var b = edge.Direction;
                var c = GeometryVector.ByTwoVertices(this.StartVertex, edge.StartVertex);
                GeometryVector cross = a.Cross(b);
                double numerator = c.Dot(cross);
                double denominator = cross.Length;
                return Math.Abs(numerator) / Math.Abs(denominator);

            }
            
        }

        #region override methods
        //TODO: Improve overriding equality methods as per http://www.loganfranken.com/blog/687/overriding-equals-in-c-part-1/

        /// <summary>
        /// Override of Equal Method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            GeometryEdge e= (GeometryEdge)obj;
            if (StartVertex.Equals(e.StartVertex) && EndVertex.Equals(e.EndVertex)) { return true; }
            if (StartVertex.Equals(e.EndVertex) && EndVertex.Equals(e.StartVertex)) { return true; }
            return false;

        }

        /// <summary>
        /// Override of GetHashCode Method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return StartVertex.GetHashCode() ^ EndVertex.GetHashCode();
        }


        /// <summary>
        /// Override of ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("gEdge(StartVertex: {0}, EndVertex: {1})", StartVertex, EndVertex);
        }

        internal override GeometryBoundingBox ComputeBoundingBox()
        {
            return GeometryBoundingBox.ByMinVertexMaxVertex(StartVertex, EndVertex);
        }

        #endregion

    }

    
    
}