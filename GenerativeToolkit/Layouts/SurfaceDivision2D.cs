﻿using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DSCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeToolkit.Layouts
{
    [IsVisibleInDynamoLibrary(false)]
    public class SurfaceDivision2D
    {
        [IsVisibleInDynamoLibrary(true)]
        public static List<Geometry> DivideSurface(Surface surface, List<double> U, List<double> V)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            List<Geometry> dividedSurfaces = new List<Geometry>();

            List<PolySurface> polySurfaces = new List<PolySurface>();
            List<List<double>> UV = new List<List<double>>{ U, V };
            Curve uCurve = Curve.ByIsoCurveOnSurface(surface, 1, 0);
            for (int i = 0; i <= 1; i++)
            {
                List<Surface> crvSurf = new List<Surface>();
                foreach (double item in UV[i])
                {
                    Curve crv = Curve.ByIsoCurveOnSurface(surface, i, item);
                    crvSurf.Add(crv.Extrude(Vector.ByCoordinates(0,0,1)));
                    crv.Dispose();
                }
                polySurfaces.Add(PolySurface.ByJoinedSurfaces(crvSurf));
                disposables.AddRange(crvSurf);
            }
            List<Geometry> splitSurfaces = surface.Split(polySurfaces[1]).ToList();
            List<Geometry> sortedSurfaces = splitSurfaces.OrderBy(x => uCurve.DistanceTo(x)).ToList();
            disposables.AddRange(splitSurfaces);

            foreach (var surf in sortedSurfaces)
            {
                dividedSurfaces.AddRange(surf.Split(polySurfaces[0]));
            }
            disposables.AddRange(sortedSurfaces);
            disposables.AddRange(polySurfaces);

            disposables.ForEach(x => x.Dispose());
            return dividedSurfaces;           
        }
    }
}