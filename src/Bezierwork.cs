using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements10BasicSample;
using Newtonsoft.Json;

namespace Elements
{
    public class Bezierwork : GeometricElement
    {
        public Bezier Bezier { get; set; }
        [JsonProperty("Add Id")]
        public string AddId { get; set; }

        public Bezierwork(BeziersOverrideAddition add)
        {
            this.Bezier = new Bezier(add.Value.Polyline.Vertices.ToList());
            this.AddId = add.Id;

            SetMaterial();
        }

        public Bezierwork(string id, Bezier bezier)
        {
            this.Bezier = bezier;
            this.AddId = id;
            SetMaterial();
        }

        public bool Match(BeziersIdentity identity)
        {
            return identity.AddId == this.AddId;
        }

        public Bezierwork Update(BeziersOverride edit)
        {
            this.Bezier = new Bezier(edit.Value.Polyline.Vertices.ToList());
            return this;
        }

        public void SetMaterial()
        {
            var materialName = this.Name + "_MAT";
            var materialColor = new Color(0.952941176, 0.360784314, 0.419607843, 1.0); // F15C6B with alpha 1
            var material = new Material(materialName);
            material.Color = materialColor;
            material.Unlit = true;
            this.Material = material;
        }

        public override void UpdateRepresentations()
        {
            var rep = new Representation();
            var solidRep = new Solid();

            // Define parameters for the extruded circle and spherical point
            var circleRadius = 0.1;
            var pointRadius = 0.2;
            var innerPointRadius = 0.05;

            // Create an sweep along the curve with a circular profile
            var circle = new Elements.Geometry.Circle(Vector3.Origin, circleRadius).ToPolygon(10);

            var sweep = new Sweep(circle, Bezier, 0, 0, 0, false);

            rep.SolidOperations.Add(sweep);

            // Add a spherical point at each vertex of the polyline
            for (int i = 0; i < Bezier.ControlPoints.Count; i++)
            {
                var vertex = Bezier.ControlPoints[i];
                var sphere = Mesh.Sphere((i == 0 || i == Bezier.ControlPoints.Count - 1) ? pointRadius : innerPointRadius, 10);

                HashSet<Geometry.Vertex> modifiedVertices = new HashSet<Geometry.Vertex>();
                // Translate the vertices of the mesh to center it at the origin
                foreach (var svertex in sphere.Vertices)
                {
                    if (!modifiedVertices.Contains(svertex))
                    {
                        svertex.Position += vertex;
                        modifiedVertices.Add(svertex);
                    }
                }

                foreach (var triangle in sphere.Triangles)
                {
                    // Create a Polygon from the triangle's vertices point
                    var polygon = new Polygon(triangle.Vertices.SelectMany(v => new List<Vector3> { v.Position }).ToList());
                    solidRep.AddFace(polygon);
                }
            }

            var consol = new ConstructedSolid(solidRep);
            rep.SolidOperations.Add(consol);

            this.Representation = rep;
        }
    }
}