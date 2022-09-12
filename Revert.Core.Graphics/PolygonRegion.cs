using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics
{
    public class PolygonRegion
    {
        public float[] TextureCoords { get; set; } // texture coordinates in atlas coordinates
        public VertexPositionTexture[] Vertices { get; set; } // pixel coordinates relative to source image.
        public short[] Triangles { get; set; }
        public TextureRegion Region { get; set; }

	/** Creates a PolygonRegion by triangulating the polygon coordinates in vertices and calculates uvs based on that.
	 * TextureRegion can come from an atlas.
	 * @param region the region used for drawing
	 * @param vertices contains 2D polygon coordinates in pixels relative to source region */
	public PolygonRegion(TextureRegion region, VertexPositionTexture[] vertices, short[] triangles)
        {
            this.Region = region;
            this.Vertices = vertices;
            this.Triangles = triangles;

            float[] textureCoords = this.TextureCoords = new float[vertices.Length];
            float u = region.u, v = region.v;
            float uvWidth = region.u2 - u;
            float uvHeight = region.v2 - v;
            int width = region.regionWidth;
            int height = region.regionHeight;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                textureCoords[i] = u + uvWidth * (vertex.Position.X / width);
                textureCoords[i + 1] = v + uvHeight * (1 - (vertex.Position.Y / height));
            }
        }



    }
}
