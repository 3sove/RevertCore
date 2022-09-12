using Revert.Core.Common.Types;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Curves;
using Revert.Core.Mathematics.Factories;
using Revert.Core.Mathematics.Geometry;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Revert.Core.Graphics.Clusters
{
    public class MapCluster
    {
        public static int maxId { get; set; }

        public List<MapItem> boundary { get; set; }

        public HashSet<Common.Types.KeyPair<int, int>> locations { get; set; } = new HashSet<Common.Types.KeyPair<int, int>>();

        public Polygon polygon { get; set; }

        public Rectangle rectangle { get; set; }

        public List<Vector2> boundaryPoints
        {
            get
            {
                var points = new List<Vector2>();
                for (int i = 0; i < boundary.Count; i++)
                {
                    var item = boundary[i];
                    points.Add(new Vector2(item.sceneX, item.sceneY));
                }
                points = Curve.simplifyPoints(points);
                return points;
            }
        }

        private Vector2 midPoint = Vec2Factory.Instance.get();

        public MapCluster(List<MapItem> items)
        {
            boundary = items;
        }

        public int size()
        {
            return boundary.Count;
        }

        public bool add(MapItem item)
        {
            boundary.Add(item);
            setPolygon(boundary);
            return true;
        }

        private void setPolygon(List<MapItem> boundary)
        {
            var vertices = new float[boundary.Count * 2];
            var i = 0;
            while (i < boundary.Count)
            {
                var item = boundary[i];
                vertices[i] = item.xIndex;
                vertices[i + 1] = item.yIndex;
                locations.Add(new Common.Types.KeyPair<int, int>(item.xIndex, item.yIndex));
                i += 2;
            }
            polygon = new Polygon(vertices);

            var xPoints = boundaryPoints.OrderBy(item => item.x);
            var yPoints = boundaryPoints.OrderBy(item => item.y);

            var left = xPoints.First();
            var right = xPoints.Last();
            var bottom = yPoints.First();
            var top = yPoints.Last();
            if (rectangle != null) rectangle.set(left.x, bottom.y, right.x - left.x, top.y - bottom.y);
            else rectangle = new Rectangle(left.x, bottom.y, right.x - left.x, top.y - bottom.y);
        }

        public Rectangle getRectangle()
        {
            if (rectangle == null)
                rectangle = boundaryPoints.getRectangle();
            return rectangle;
        }

        public bool contains(MapItem item)
        {
            return locations.Contains(new Common.Types.KeyPair<int, int>(item.xIndex, item.yIndex)) || polygon.Contains(item.xIndex, item.yIndex);
        }

        public Vector2 getMidPoint()
        {
            if (this.boundary.Count == 0) return midPoint;
            var x = 0f;
            var y = 0f;
            foreach (var item in this.boundary)
            {
                x += item.sceneX;
                y += item.sceneY;
            }
            return midPoint.set(x / this.boundary.Count, y / this.boundary.Count);
        }
    }
}