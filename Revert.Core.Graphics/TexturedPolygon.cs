using Microsoft.Xna.Framework.Graphics;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.Graphics
{
    public static class TexturedPolygon
    {
        public static PolygonSprite getTexturedPolygon(List<Vector2> vertices, Texture2D texture)
        {
            //var vertexFan = Vertices.GetVertexFan(vertices);
            var model = Polygons.GetPolygonModel(vertices);

            var borderVertices = model.BorderVertices.Select(item =>
            new VertexPositionTexture(new Microsoft.Xna.Framework.Vector3(item.x, item.y, 0f),
            new Microsoft.Xna.Framework.Vector2(item.x, item.y))).ToArray();

            var polygonRegion = new PolygonRegion(new TextureRegion(texture), borderVertices, model.TriangleIndices);
            GraphicsDevice newGraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, new PresentationParameters());
            return new PolygonSprite(polygonRegion, newGraphicsDevice);
        }
    }
}
