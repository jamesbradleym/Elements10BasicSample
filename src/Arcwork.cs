using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements10BasicSample;
using Newtonsoft.Json;

namespace Elements
{
    public class Arcwork : GeometricElement
    {
        public Arc Arc { get; set; }
        [JsonProperty("Add Id")]
        public string AddId { get; set; }

        public Arcwork(string id, Arc arc)
        {
            this.Arc = arc;
            this.AddId = id;
            SetMaterial();
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

            // Define parameters for the 3D circlework and spherical point
            var circleRadius = 0.1;
            var pointRadius = 0.2;

            var arcVertices = new List<Vector3>() { Arc.Start, Arc.End };

            var direction = Arc.PointAt(0) - Arc.PointAt(0.1);

            var circle = new Elements.Geometry.Circle(Vector3.Origin, circleRadius).ToPolygon(10);

            // Create an swept circle along the circle
            var sweep = new Sweep(circle, Arc, 0, 0, 0, false);

            rep.SolidOperations.Add(sweep);

            // Add a spherical point at each specified point of the Circle
            foreach (var vertex in arcVertices)
            {
                var sphere = Mesh.Sphere(pointRadius, 10);

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