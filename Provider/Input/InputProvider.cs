using Glacier.Common.Engine;
using Glacier.Common.Provider;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider.Input
{
    /// <summary>
    /// Handles input in a lightweight and simple manner
    /// </summary>
    public sealed class InputProvider : IProvider
    {
        public enum TransformGroup
        {
            World,
            Untransformed
        }
        /// <summary>
        /// InputEvent arguments class.
        /// </summary>
        public class InputEventArgs
        {
            /// <summary>
            /// Whether another element has already seen this event
            /// </summary>
            public bool Handled { get; set; } = false;
            /// <summary>
            /// Currently pressed keys
            /// </summary>
            public Keys[] PressedKeys
            {
                get; internal set;
            }
            /// <summary>
            /// Mouse LeftButton was pressed and then released.
            /// </summary>
            public bool MouseLeftClick
            {
                get; internal set;
            }
            /// <summary>
            /// Mouse RightButton was pressed and then released.
            /// </summary>
            public bool MouseRightClick
            {
                get; internal set;
            }
            /// <summary>
            /// Was the mouse moved on this frame?
            /// </summary>
            public bool MouseMoved
            {
                get; internal set;
            }
            /// <summary>
            /// The current mouse position in screen coordinates
            /// </summary>
            public Point Position
            {
                get; internal set;
            }
        }
        public delegate void InputEventHandler(InputEventArgs e);
        /// <summary>
        /// When an input is observed this event is triggered.
        /// </summary>
        public event InputEventHandler InputEvent;
        /// <summary>
        /// For controls that require to be observed before other controls, this is exactly the same as <see cref="InputEvent"/>, however Handled carries over to <see cref="InputEvent"/>
        /// </summary>
        public event InputEventHandler PreviewInputEvent;
        public bool MouseLeftDown { get; private set; } = false;
        public bool MouseRightDown { get; private set; } = false;
        public ProviderManager Parent { get; set; }
        public bool MouseMoved { get; private set; }
        public bool IsLoaded { get; set; } = true;
        /// <summary>
        /// The input event args for this frame
        /// </summary>
        public InputEventArgs CurrentArgs { get; private set; }
        private bool _manualHandle = false;

        Keys[] pressedKeys = new Keys[0];
        Point oldMousePosition;
        Dictionary<IClickable, TransformGroup> subscribers = new Dictionary<IClickable, TransformGroup>();

        public void Listen()
        {
            bool left = false, right = false;
            List<Keys> finalizekeys = new List<Keys>();
            var mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Pressed)
            {
                MouseLeftDown = true;
            }
            else if (MouseLeftDown)
            {
                MouseLeftDown = false;
                left = true;
            }
            if (mState.RightButton == ButtonState.Pressed)
            {
                MouseRightDown = true;
            }
            else if (MouseRightDown)
            {
                MouseRightDown = false;
                right = true;
            }
            var kState = Keyboard.GetState();
            var nowPressed = kState.GetPressedKeys();
            foreach (var key in nowPressed)
                if (!pressedKeys.Contains(key))
                    finalizekeys.Add(key);
            pressedKeys = nowPressed.ToArray();
            MouseMoved = oldMousePosition != GameResources.MouseWorldPosition;
            CurrentArgs = new InputEventArgs()
            {
                PressedKeys = finalizekeys.ToArray(),
                MouseLeftClick = left,
                MouseRightClick = right,
                Position = Mouse.GetState().Position,
                MouseMoved = MouseMoved
            };
            oldMousePosition = GameResources.MouseWorldPosition;
        }

        /// <summary>
        /// Subscribes the object to the mouse collision check system
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Clickable"></param>
        public void Subscribe<T>(T Clickable, TransformGroup Group = TransformGroup.World) where T : IClickable
        {
            subscribers.Add(Clickable, Group);
        }

        /// <summary>
        /// Unsubscribes the object from the mouse collision check system
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Clickable"></param>
        public void Unsubscribe<T>(T Clickable) where T : IClickable
        {
            subscribers.Remove(Clickable);
        }

        public T CollisionCheck<T>(GameTime time, IDictionary<T, TransformGroup> CheckThrough, out IEnumerable<T> Results) where T : IClickable
        {
            var MouseState = Mouse.GetState();
            var results = new List<T>();
            bool singularCheck = true;
            var count = 0;
            foreach (var tupleObj in CheckThrough.Reverse())
            {
                if (tupleObj.Key.Destroyed)
                {
                    Unsubscribe(tupleObj.Key);
                    count++;
                }
                if (!tupleObj.Key.Enabled) { tupleObj.Key.IsMouseOver = false; continue; }
                var mousePos = MouseState.Position;
                if (tupleObj.Value == TransformGroup.World)
                    mousePos = GameResources.MouseWorldPosition;
                var MouseRect = new Rectangle(mousePos, new Point(1, 1));
                var x = tupleObj.Key;
                var oldMouseOver = x.IsMouseOver;
                if (x.Hitbox.Intersects(MouseRect) && (singularCheck ? !results.Any() : true)) //Per-Pixel detection (fast)
                {
                    if (x.Texture != null && x.PerpixelHitDetection)
                    {
                        var pt = (mousePos - x.Hitbox.Location).ToVector2() + x.Safezone.Location.ToVector2();
                        if (x.Scale != 1)
                            pt /= new Vector2((float)x.Scale);
                        var data = new Color[1];
                        x.Texture.GetData(0, new Rectangle(pt.ToPoint(), new Point(1, 1)), data, 0, 1);
                        if (data[0] != Color.Transparent)
                            x.IsMouseOver = true;
                        else x.IsMouseOver = false;
                    }
                    else x.IsMouseOver = true;
                    if (x.IsMouseOver)
                    {
                        results.Add(x);
                        if (oldMouseOver != x.IsMouseOver)
                            x.MouseEnter(time, CurrentArgs);
                        if (CurrentArgs.MouseLeftClick)// || CurrentArgs.MouseRightClick)
                            x.Clicked(time, CurrentArgs);
                        if (MouseState.LeftButton == ButtonState.Pressed || MouseState.RightButton == ButtonState.Pressed)
                            x.MouseDown(time, CurrentArgs);
                    }
                }
                else
                {
                    x.IsMouseOver = false;
                    if (oldMouseOver != x.IsMouseOver)
                        x.MouseLeave(time, CurrentArgs);
                }
            }
            if (count > 0)
                Debug.WriteLine("[GLACIER]: InputProvider cleaned " + count + " destroyed subscribers. (" + subscribers.Count + " subscribers)");
            Results = results;
            return results.FirstOrDefault();
        }

        public void Refresh(GameTime gt)
        {
            Listen();
            if (CurrentArgs.MouseLeftClick || CurrentArgs.MouseRightClick || CurrentArgs.PressedKeys.Any())
                PreviewInputEvent?.Invoke(CurrentArgs);
            CollisionCheck(gt, subscribers, out IEnumerable<IClickable> results);
            if (!CurrentArgs.Handled)
                CurrentArgs.Handled = results.Any();            
            if (CurrentArgs.MouseLeftClick || CurrentArgs.MouseRightClick || CurrentArgs.PressedKeys.Any())
                InputEvent?.Invoke(CurrentArgs);
            _manualHandle = false;
        }
    }
}

