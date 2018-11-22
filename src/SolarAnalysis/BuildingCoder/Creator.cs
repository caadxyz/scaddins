//
// Creator.cs - model line creator helper class
//
// Copyright (C) 2008-2018 by Jeremy Tammik,
// Autodesk Inc. All rights reserved.
//
// Keywords: The Building Coder Revit API C# .NET add-in.
//

namespace BuildingCoder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.Revit.DB;

    internal class Creator
    {
        // these are
        // Autodesk.Revit.Creation
        // objects!
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Microsoft.Usage", "CA2213: Disposable fields should be disposed", Justification = "Parameter initialized by Revit", MessageId = "creapp")]
        private Autodesk.Revit.Creation.Application creapp;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Microsoft.Usage", "CA2213: Disposable fields should be disposed", Justification = "Parameter initialized by Revit", MessageId = "credoc")]
        private Autodesk.Revit.Creation.Document credoc;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Microsoft.Usage", "CA2213: Disposable fields should be disposed", Justification = "Parameter initialized by Revit", MessageId = "doc")]
        private Document doc;

        public Creator(Document doc)
        {
            this.doc = doc;
            credoc = doc.Create;
            creapp = doc.Application.Create;
        }

        /// <summary>
        /// Create a model line between the two given points.
        /// Internally, it creates an arbitrary sketch
        /// plane given the model line end points.
        /// </summary>
        public static ModelLine CreateModelLine(
          Document doc,
          XYZ p,
          XYZ q)
        {
            if (p.DistanceTo(q) < Util.MinLineLength) {
                return null;
            }

            //// Create sketch plane; for non-vertical lines,
            //// use Z-axis to span the plane, otherwise Y-axis:

            XYZ v = q - p;

            double dxy = Math.Abs(v.X) + Math.Abs(v.Y);

            XYZ w = (dxy > Util.TolPointOnPlane) ? XYZ.BasisZ : XYZ.BasisY;

            XYZ norm = v.CrossProduct(w).Normalize();

            ////Autodesk.Revit.Creation.Application creApp
            ////  = doc.Application.Create;

            ////Plane plane = creApp.NewPlane( norm, p ); // 2014
#if REVIT2016
            Plane plane = new Plane(norm, p); // 2015, 2016
#else
            Plane plane = Plane.CreateByNormalAndOrigin(norm, p); // 2017
#endif

            ////SketchPlane sketchPlane = creDoc.NewSketchPlane( plane ); // 2013
            SketchPlane sketchPlane = SketchPlane.Create(doc, plane); // 2014

            ////Line line = creApp.NewLine( p, q, true ); // 2013
            Line line = Line.CreateBound(p, q); // 2014

            //// The following line is only valid in a project
            //// document. In a family, it will throw an exception
            //// saying "Document.Create can only be used with
            //// project documents. Use Document.FamilyCreate
            //// in the Family Editor."

            ////Autodesk.Revit.Creation.Document creDoc
            ////  = doc.Create;

            ////return creDoc.NewModelCurve(
            ////  //creApp.NewLine( p, q, true ), // 2013
            ////  Line.CreateBound( p, q ), // 2014
            ////  sketchPlane ) as ModelLine;

            ModelCurve curve = doc.IsFamilyDocument
              ? doc.FamilyCreate.NewModelCurve(line, sketchPlane)
              : doc.Create.NewModelCurve(line, sketchPlane);

            return curve as ModelLine;
        }

        public ModelCurve CreateModelCurve(Curve curve)
        {
            return credoc.NewModelCurve(curve, NewSketchPlaneContainCurve(curve));
        }

        public ModelCurveArray CreateModelCurves(
                  Curve curve)
        {
            var array = new ModelCurveArray();

            var line = curve as Line;
            if (line != null) {
                array.Append(CreateModelLine(doc, curve.GetEndPoint(0), curve.GetEndPoint(1)));
                return array;
            }

            var arc = curve as Arc;
            if (arc != null) {
                var origin = arc.Center;
                var normal = arc.Normal;

                array.Append(CreateModelCurve(arc, origin, normal));

                return array;
            }

            var ellipse = curve as Ellipse;
            if (ellipse != null) {
                var origin = ellipse.Center;
                var normal = ellipse.Normal;

                array.Append(CreateModelCurve(
                  ellipse, origin, normal));

                return array;
            }

            var points = curve.Tessellate();
            var p = points.First();

            foreach (var q in points.Skip(1)) {
                array.Append(CreateModelLine(doc, p, q));
                p = q;
            }

            return array;
        }

        public void DrawFaceTriangleNormals(Face f)
        {
            Mesh mesh = f.Triangulate();
            int n = mesh.NumTriangles;

            string s = "{0} face triangulation returns "
              + "mesh triangle{1} and normal vector{1}:";

            Debug.Print(
              s, n, Util.PluralSuffix(n));

            for (int i = 0; i < n; ++i) {
                MeshTriangle t = mesh.get_Triangle(i);

                XYZ p = (t.get_Vertex(0)
                  + t.get_Vertex(1)
                  + t.get_Vertex(2)) / 3;

                XYZ v = t.get_Vertex(1)
                  - t.get_Vertex(0);

                XYZ w = t.get_Vertex(2)
                  - t.get_Vertex(0);

                XYZ normal = v.CrossProduct(w).Normalize();

                Debug.Print(
                  "{0} {1} --> {2}",
                  i,
                  Util.PointString(p),
                  Util.PointString(normal));

                CreateModelLine(doc, p, p + normal);
            }
        }

        public void DrawPolygon(List<XYZ> loop)
        {
            XYZ p1 = XYZ.Zero;
            XYZ q = XYZ.Zero;
            bool first = true;
            foreach (XYZ p in loop) {
                if (first) {
                    p1 = p;
                    first = false;
                } else {
                    CreateModelLine(doc, p, q);
                }
                q = p;
            }
            CreateModelLine(doc, q, p1);
        }

        public void DrawPolygons(
                  List<List<XYZ>> loops)
        {
            foreach (List<XYZ> loop in loops) {
                DrawPolygon(loop);
            }
        }

        private ModelCurve CreateModelCurve(
                  Curve curve,
                  XYZ origin,
                  XYZ normal)
        {
#if !REVIT2016
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin); // 2017

            SketchPlane sketchPlane = SketchPlane.Create(
              doc, plane);

            return credoc.NewModelCurve(
              curve, sketchPlane);
#else
            return null;
#endif
        }

        /// <summary>
        /// Determine the plane that a given curve resides in and return its normal vector.
        /// Ask the curve for its start and end points and some point in the middle.
        /// The latter can be obtained by asking the curve for its parameter range and
        /// evaluating it in the middle, or by tessellation. In case of tessellation,
        /// you could iterate through the tessellation points and use each one together
        /// with the start and end points to try and determine a valid plane.
        /// Once one is found, you can add debug assertions to ensure that the other
        /// tessellation points (if there are any more) are in the same plane.
        /// In the case of the line, the tessellation only returns two points.
        /// I once heard that that is the only element that can do that, all
        /// non-linear curves return at least three. So you could use this property
        /// to determine that a line is a line (and add an assertion as well, if you like).
        /// Update, later: please note that the Revit API provides an overload of the
        /// NewPlane method taking a CurveArray argument.
        /// </summary>
        private XYZ GetCurveNormal(Curve curve)
        {
            IList<XYZ> pts = curve.Tessellate();
            int n = pts.Count;

            Debug.Assert(n > 1, "expected at least two points " + "from curve tessellation");

            XYZ p = pts[0];
            XYZ q = pts[n - 1];
            XYZ v = q - p;
            XYZ w, normal = null;

            ////if (n == null) {
            ////    Debug.Assert(curve is Line, "expected non-line element to have " + "more than two tessellation points");
            ////    double dxy = Math.Abs(v.X) + Math.Abs(v.Y);
            ////    w = (dxy > Util.TolPointOnPlane) ? XYZ.BasisZ : XYZ.BasisY;
            ////    normal = v.CrossProduct(w).Normalize();
            ////} else {
            int i = 0;
            while (++i < n - 1) {
                w = pts[i] - p;
                normal = v.CrossProduct(w);
                if (!normal.IsZeroLength()) {
                    normal = normal.Normalize();
                    break;
                }
            }
            ////}
            return normal;
        }

        /// <summary>
        /// Return a new sketch plane containing the given curve.
        /// Update, later: please note that the Revit API provides
        /// an overload of the NewPlane method taking a CurveArray
        /// argument, which could presumably be used instead.
        /// </summary>
        private SketchPlane NewSketchPlaneContainCurve(
          Curve curve)
        {
#if !REVIT2016
            XYZ p = curve.GetEndPoint(0);
            XYZ normal = GetCurveNormal(curve);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, p); // 2017
            return SketchPlane.Create(doc, plane); // 2014
#else
            return null;
#endif
        }

        private SketchPlane NewSketchPlanePassLine(
                  Line line)
        {
            XYZ p = line.GetEndPoint(0);
            XYZ q = line.GetEndPoint(1);
            XYZ norm;
            if (p.X == q.X) {
                norm = XYZ.BasisX;
            } else if (p.Y == q.Y) {
                norm = XYZ.BasisY;
            } else {
                norm = XYZ.BasisZ;
            }
#if REVIT2016
            ////Plane plane = _creapp.NewPlane( norm, p ); // 2016
            return null;
#else
            Plane plane = Plane.CreateByNormalAndOrigin(norm, p); // 2017
             return SketchPlane.Create(doc, plane); // 2014
#endif
            ////return _credoc.NewSketchPlane( plane ); // 2013
        }
    }
}