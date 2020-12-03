using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThatStings
{
    public class Screen : GameObject
    {
        float opacity = 0;
        Texture2D currentSprite;
        public Screen(Texture2D sprite, float startOp)
        {
            currentSprite = sprite;
            opacity = startOp;
            position = Vector2.Zero;
        }

        public override void Update(GameTime gameTime)
        {

            opacity += 0.01f;
            if (opacity > 1f)
            {
                opacity = 1;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(currentSprite, position, null, Color.White * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
