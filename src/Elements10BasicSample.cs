using Elements;
using Elements.Geometry;
using System.Collections.Generic;

namespace Elements10BasicSample
{
    public static class Elements10BasicSample
    {
        /// <summary>
        /// The Elements20Sample function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A Elements20SampleOutputs instance containing computed results and the model with any new elements.</returns>
        public static Elements10BasicSampleOutputs Execute(Dictionary<string, Model> inputModels, Elements10BasicSampleInputs input)
        {
            var output = new Elements10BasicSampleOutputs();

            List<object> curves = new List<object>();

            /// CIRCLE
            // Create a circle
            var circle = new Circle(new Vector3(5, 5, 0), 4);
            var circlework = new Circlework("Circle Example", circle);
            output.Model.AddElement(circlework);

            /// ARC
            // Create an arc
            var arc = new Arc(new Vector3(15, 5, 0), 4, 0.0, 270.0);
            var arcwork = new Arcwork("Arc Example", arc);
            output.Model.AddElement(arcwork);

            /// ELLIPSE
            /// N/A

            /// BEZIER
            // Create a bezier
            var bezier = new Bezier(
                new List<Vector3>()
                {
                    new Vector3(31, 5, 0),
                    new Vector3(35, 20, 0),
                    new Vector3(35, -10, 0),
                    new Vector3(39, 5.01, 0),
                }
            );
            var bezierPolyline = new Polyline(
              new List<Vector3>()
              {
                  new Vector3(31, 5, 0),
                  new Vector3(35, 20, 0),
                  new Vector3(35, -10, 0),
                  new Vector3(39, 5.01, 0),
              }
            );
            input.Overrides.Additions.Beziers.Add(new BeziersOverrideAddition("Bezier Example", new BeziersIdentity("Bezier Example"), new BeziersOverrideAdditionValue(bezierPolyline)));

            // Create bezierworks via override input
            var bezierworks = input.Overrides.Beziers.CreateElements(
              input.Overrides.Additions.Beziers,
              input.Overrides.Removals.Beziers,
              (add) => new Bezierwork(add),
              (bezierwork, identity) => bezierwork.Match(identity),
              (bezierwork, edit) => bezierwork.Update(edit)
            );


            output.Model.AddElements(bezierworks);

            /// LINE
            // Create a line
            var line = new Line(new Vector3(45, 1, 0), new Vector3(45, 9, 0));
            input.Overrides.Additions.Lines.Add(new LinesOverrideAddition("Line Example", new LinesIdentity("Line Example"), new LinesOverrideAdditionValue(line)));

            // Create lineworks via override input
            var lineworks = input.Overrides.Lines.CreateElements(
                input.Overrides.Additions.Lines,
                input.Overrides.Removals.Lines,
                (add) => new Linework(add),
                (linework, identity) => linework.Match(identity),
                (linework, edit) => linework.Update(edit)
            );

            // Add drawn lineworks to the model
            output.Model.AddElements(lineworks);

            /// POLYLINE
            // Create a polyline
            var polyline = new Polyline(
                new List<Vector3>()
                {
                    new Vector3(56.5, 9, 0),
                    new Vector3(54, 6.5, 0),
                    new Vector3(55, 6.5, 0),
                    new Vector3(52, 4, 0),
                    new Vector3(53, 4, 0),
                    new Vector3(51, 1, 0),
                    new Vector3(55.5, 4.5, 0),
                    new Vector3(54.5, 4.5, 0),
                    new Vector3(57.5, 7, 0),
                    new Vector3(56.5, 7, 0),
                    new Vector3(59, 9, 0),
                }
            );
            input.Overrides.Additions.Polylines.Add(new PolylinesOverrideAddition("Polyline Example", new PolylinesIdentity("Polyline Example"), new PolylinesOverrideAdditionValue(polyline)));

            // Create polylineworks via override input
            var polylineworks = input.Overrides.Polylines.CreateElements(
              input.Overrides.Additions.Polylines,
              input.Overrides.Removals.Polylines,
              (add) => new Polylinework(add),
              (polylinework, identity) => polylinework.Match(identity),
              (polylinework, edit) => polylinework.Update(edit)
            );

            // Add drawn polylineworks to the model
            output.Model.AddElements(polylineworks);

            curves.Add(circlework);
            curves.Add(arcwork);
            curves.AddRange(bezierworks);

            curves.AddRange(lineworks);
            curves.AddRange(polylineworks);

            var parameter = input.Parameter;
            var directionMod = 0.01;
            var size = 1.0;
            var subsize = 0.5;

            var parameterMaterial = new Material("Parameter Material", new Color(0, 0, 0, 0.5));
            var midMaterial = new Material("Mid Material", new Color(0, 0.5, 0, 0.5));
            var segmentMaterial = new Material("Segment Material", new Color(0.5, 0, 0, 0.5));
            var segmentMidMaterial = new Material("Segment Mid Material", new Color(0, 0, 0.5, 0.5));
            foreach (var curve in curves)
            {
                if (curve is Polylinework _polylinework)
                {
                    // Find the point at the given parameter
                    // Polyline parameterization in Elements 1.0 is domain based, 0->1
                    var point = _polylinework.Polyline.PointAt(parameter);
                    // Find an appropriate direction to orient our mass
                    var direction = _polylinework.Polyline.PointAt(parameter) - _polylinework.Polyline.PointAt(parameter + directionMod);
                    var mass = MassAtPointAndOrientation(size, point, direction);
                    output.Model.AddElement(mass);

                    var midpoint = _polylinework.Polyline.PointAt(0.5);
                    // Find an appropriate direction to orient our mass
                    var middirection = _polylinework.Polyline.PointAt(0.5) - _polylinework.Polyline.PointAt(0.5 + directionMod);
                    var midmass = MassAtPointAndOrientation(size, midpoint, middirection, midMaterial);
                    output.Model.AddElement(midmass);

                    if (_polylinework.Polyline.Segments().Count() > 1)
                    {
                        foreach (var segment in _polylinework.Polyline.Segments())
                        {
                            // Find the point at the given parameter
                            // Line parameterization in Elements 1.0 is domain based, 0->1
                            var segmentpoint = segment.PointAt(parameter);
                            // Find an appropriate direction to orient our mass
                            var segmentdirection = segment.Direction();
                            var segmentmass = MassAtPointAndOrientation(subsize, segmentpoint, segmentdirection, segmentMaterial);
                            output.Model.AddElement(segmentmass);

                            // Find the midpoint
                            // Line parameterization in Elements 1.0 is domain based, 0->1
                            var segmentmidpoint = segment.PointAt(0.5);
                            var segmentmidmass = MassAtPointAndOrientation(subsize, segmentmidpoint, segmentdirection, segmentMidMaterial);
                            output.Model.AddElement(segmentmidmass);
                        }
                    }

                }
                else if (curve is Linework _linework)
                {
                    // Find the point at the given parameter
                    // Line parameterization is domain based, 0->1
                    var point = _linework.Line.PointAt(parameter);
                    // Find an appropriate direction to orient our mass
                    var direction = _linework.Line.Direction();
                    var mass = MassAtPointAndOrientation(size, point, direction);
                    output.Model.AddElement(mass);
                }
                else if (curve is Bezierwork _bezierwork)
                {
                    // Find the point at the given parameter
                    // Bezier parameterization is domain based, 0->1
                    var point = _bezierwork.Bezier.PointAt(parameter);
                    // Find an appropriate direction to orient our mass
                    var direction = _bezierwork.Bezier.PointAt(parameter) - _bezierwork.Bezier.PointAt(parameter + directionMod);
                    var mass = MassAtPointAndOrientation(size, point, direction);
                    output.Model.AddElement(mass);
                }
                else if (curve is Circlework _circlework)
                {
                    // Find the point at the given parameter
                    // Circle parameterization is domain based, 0->1
                    var point = _circlework.Circle.PointAt(parameter);
                    // Find an appropriate direction to orient our mass
                    var direction = _circlework.Circle.PointAt(parameter) - _circlework.Circle.PointAt(parameter + directionMod);
                    var mass = MassAtPointAndOrientation(size, point, direction);
                    output.Model.AddElement(mass);
                }
                else if (curve is Arcwork _arcwork)
                {
                    // Find the point at the given parameter
                    // Arc parameterization is domain based, 0->1
                    var point = _arcwork.Arc.PointAt(parameter);
                    // Find an appropriate direction to orient our mass
                    var direction = _arcwork.Arc.PointAt(parameter) - _arcwork.Arc.PointAt(parameter + directionMod);
                    var mass = MassAtPointAndOrientation(size, point, direction);
                    output.Model.AddElement(mass);
                }
            }
            return output;
        }


        public static Mass MassAtPointAndOrientation(Double size, Vector3 point, Vector3 direction, Material? material = null)
        {
            // Create a Mass of specified size
            var mass = new Mass(Polygon.Rectangle(size, size), size);
            // Find the 3D center of the mass
            var center = mass.Bounds.Center() + new Vector3(0, 0, size / 2.0) + mass.Transform.Origin;
            // Transform the mass to the designated point and direction
            mass.Transform = new Transform(-1 * center).Concatenated(new Transform(new Plane(point, direction)));
            // Set the mass material if specified
            if (material != null)
            {
                mass.Material = material;
            }
            return mass;
        }
    }
}