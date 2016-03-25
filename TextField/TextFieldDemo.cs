using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TextField
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TextFieldDemo : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch _screen;

        public TextFieldDemo()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _views = new List<IDrawable>();
            // Create a new SpriteBatch, which can be used to draw textures.
            _screen = new SpriteBatch(GraphicsDevice);

            var font = Content.Load<SpriteFont>("text");
            var textFieldBg = Content.Load<Texture2D>("text_bg");

            var centered = new Vector2
            {
                X = graphics.PreferredBackBufferWidth / 2f - textFieldBg.Width / 2f,
                Y = graphics.PreferredBackBufferHeight / 2f - textFieldBg.Height / 2f
            };

            _textField = new TextField(this, "TEST");

            Components.Add(_textField);
            _views.Add(new TextFieldView(
                _textField, textFieldBg, font, centered, _screen));
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _screen.Begin();
            foreach (IDrawable view in _views)
            {
                view.Draw(gameTime);
            }
            _screen.End();

            base.Draw(gameTime);
        }

        private TextField _textField;
        private List<IDrawable> _views;
    }
}
