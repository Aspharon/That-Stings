using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThatStings
{
    class Pixel : GameObject
    {
        float opacity = 1;
        Vector2 offset = new Vector2(140, 156);
        Color color;
        int scale = 4;

        public Pixel(Vector2 pos, Color col)
        {
            position = pos * scale + offset;
            color = col;
        }

        public override void Update(GameTime gameTime)
        {
            opacity -= 0.01f;
            if (opacity < 0f)
            {
                opacity = 0;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { color });
            spriteBatch.Draw(pixel, position, null, Color.White * opacity, 0, Vector2.Zero, scale, 0, 0);
        }
    }
}
