// ----------------------------------------------------------------------------
//  Copyright © 2016 Schell Games, LLC. All Rights Reserved. 
// 
//  Author: Eric Policaro
// 
//  Date: 03/25/2016
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TextField
{
    public class TextFieldView : IDrawable
    {
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        
        public TextFieldView(TextField subject, 
                             Texture2D bg,
                             SpriteFont font,
                             Vector2 pos,
                             SpriteBatch screen)
        {
            _bg = bg;
            _font = font;
            _pos = pos;
            _screen = screen;
            _subject = subject;
            _subject.TextChanged += OnTextChanged;
            Rectangle cursor = _bg.Bounds;
            cursor.Location = pos.ToPoint();
            _cursor = new Cursor(_screen, cursor);

            Border = 10f;
            Foreground = new Color(236, 240, 241, 255);
            UpdateBufferPosition();
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            UpdateBufferPosition();   
        }

        private void UpdateBufferPosition()
        {
            Vector2 size = _font.MeasureString(_subject.Buffer);
            BufferOffset = new Vector2
            {
                X = Border + _pos.X,
                Y = _pos.Y + (_bg.Height / 2f - size.Y / 2f)
            };

            _cursor.Offset = Border + size.X;
        }

        public Color Foreground
        {
            get;
            set;
        }

        public int DrawOrder
        {
            get;
            set;
        }

        public bool Visible
        {
            get;
            set;
        }

        public float Border
        {
            get;
            set;
        }

        private Vector2 BufferOffset
        {
            get;
            set;
        }

        public void Draw(GameTime gameTime)
        {
            _screen.Draw(_bg, _pos, Color.White);
            _screen.DrawString(_font, _subject.Buffer, BufferOffset, Foreground);
            _cursor.Draw(gameTime);
        }

        private readonly Cursor _cursor;
        private readonly TextField _subject;
        private readonly SpriteBatch _screen;
        private readonly Vector2 _pos;
        private readonly Texture2D _bg;
        private readonly SpriteFont _font;
    }

    public class Cursor
    {
        public Cursor(SpriteBatch screen, Rectangle container)
        {
            Color = Color.Black;
            _screen = screen;
            _height = (container.Height / 2f);
            _pos = new Vector2
            {
                X = container.X,
                Y = container.Y + (container.Height / 2f - _height /2f)
            };
        }

        public float Offset
        {
            get;
            set;
        }

        public Color Color
        {
            get;
            set;
        }

        public void Draw(GameTime gameTime)
        {
            float elapsed = (float)gameTime.TotalGameTime.TotalSeconds - _timer;
            if (elapsed > _cursorBlink)
            {
                _timer = (float)gameTime.TotalGameTime.TotalSeconds;
                _showCursor = !_showCursor;
            }

            if (_showCursor)
            {
                _screen.DrawLine(Current, _height, _angle, Color, Thickness);
            }
        }

        private Vector2 Current
        {
            get { return new Vector2(_pos.X + Offset + Thickness, _pos.Y); }
        }

        private Vector2 _pos;
        
        private bool _showCursor = true;
        private float _timer;
        private float _cursorBlink = 0.8f;

        private readonly float _angle = MathHelper.ToRadians(90f);
        private readonly float _height;
        private readonly SpriteBatch _screen;

        private const float Thickness = 2f;
    }

    public class TextField : GameComponent
    {
        public event EventHandler TextChanged = delegate { }; 

        public TextField(Game g) : this(g, "")
        {
        }

        public TextField(Game g, string startingText)
            : base(g)
        {
            Enabled = true;
            _keyMap = new Dictionary<Keys, Func<string, bool, string>>();
            Buffer = startingText;
            MaxLength = 15;
            ProcessKeys();
        }

        public int MaxLength
        {
            get;
            set;
        }

        public string Buffer
        {
            get;
            private set;
        }

        public override void Update(GameTime gameTime)
        {
            var current = Keyboard.GetState();
            Keys[] pressed = current.GetPressedKeys();
            if (pressed.Length == 0)
            {
                _timer = 0f;
                return;
            }

            Keys k = pressed[0];
            bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
            if (k == _held)
            {
                double elapsed = gameTime.TotalGameTime.TotalSeconds - _timer;
                if (elapsed > HoldTime)
                {
                    AppendText(_held, gameTime.TotalGameTime.TotalSeconds, shift);
                }
            }
            else if (_keyMap.ContainsKey(k))
            {
                _held = k;
                AppendText(_held, gameTime.TotalGameTime.TotalSeconds, shift);
            }
        }

        private void AppendText(Keys k, double seconds, bool shiftDown)
        {
            _timer = seconds;
            string old = Buffer;
            Buffer = _keyMap[k](Buffer, shiftDown);
            if (Buffer.Length > MaxLength)
                Buffer = old;

            FireTextChanged();
        }

        private Keys _held;

        private double _timer;
        private const double HoldTime = 0.2f;

        private void FireTextChanged()
        {
            TextChanged(this, EventArgs.Empty);
        }

        private void ProcessKeys()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                string s = key.ToString();
                if (s.Length == 1)
                {
                    _keyMap[key] = (buffer, shift) =>
                    {
                        return buffer + (shift ? s : s.ToLower());
                    };
                }
            }

            _keyMap[Keys.Back] = (buffer, shift) =>
            {
                if (buffer.Length == 0)
                    return "";

                return buffer.Substring(0, buffer.Length - 1);
            };

            for (int i = 0; i < 10; i++)
            {
                Keys num = (Keys)Enum.Parse(typeof(Keys), "NumPad" + i, true);
                string si = i.ToString();
                _keyMap[num] = (buffer, shift) => buffer + si;

                num = (Keys)Enum.Parse(typeof(Keys), "D" + i, true);
                _keyMap[num] = (buffer, shift) => buffer + si;
            }
        }

        private readonly Dictionary<Keys, Func<string, bool, string>> _keyMap;
    }
}