using Glacier.Common.Engine;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public class Particle : GameObject
    {
        public event EventHandler<Particle> ParticleExpired;

        public string Name
        {
            get; set;
        }
        public bool Paused { get; private set; }
        public Vector2 StartPoint
        {
            get; 
        }
        /// <summary>
        /// The current Velocity of the <see cref="Particle"/> -- measured in px/s
        /// </summary>
        public Vector2 Velocity
        {
            get;set;
        }
        public Vector2 MaxVelocity
        {
            get; set;
        } = new Vector2(300, 300);    
        /// <summary>
        /// The change in Velocity per frame -- measured in px/s^2
        /// </summary>
        public Vector2 Acceleration
        {
            get; set;
        }

        /// <summary>
        /// The amount of time before this particle will disappear
        /// </summary>
        public TimeSpan? ExpireTime
        {
            get; set;
        }
        public TimeSpan Lifespan { get; private set; }
        public bool Expired
        {
            get => _expired; 
            set
            {
                if (!_expired && value)
                    ParticleExpired?.Invoke(this, this);
                _expired = value;
            }
        }
        public ParticleProvider Parent { get; internal set; }
        public new Color Color
        {
            get => StartColor; set => StartColor = value;
        }
        private Color StartColor = Color.White;
        public ZoomFadeModes ZoomFadeMode
        {
            get;set;
        }
        /// <summary>
        /// The Zoom at which the particle appears/disappears. <see cref=""/>
        /// </summary>
        public float ZoomThreshold
        {
            get; set;
        }
        public enum ZoomFadeModes
        {
            /// <summary>
            /// Camera zoom does not affect this <see cref="Particle"/>
            /// </summary>
            Off,
            /// <summary>
            /// The <see cref="Particle"/> will appear when the Camera Zoom is greater than the ZoomThreshold 
            /// </summary>
            Appear,
            /// <summary>
            /// The <see cref="Particle"/> will disappear when the Camera Zoom is greater than the ZoomThreshold 
            /// </summary>
            Disappear
        }
        /// <summary>
        /// The range (relative to <see cref="ZoomThreshold"/> in which the <see cref="Particle"/> will fade in/out with zoom changes.
        /// </summary>
        public float ZoomFadeRange
        {
            get; set;
        } = .25f;

        private bool _expired;

        /// <summary>
        /// The velocity of the rotation, measured in deg/s
        /// </summary>
        public float RotationVelocity { get; set; }
        /// <summary>
        /// The acceleration of the rotation, measured in deg/s^2
        /// </summary>
        public float RotationAcceleration { get; set; }
        public float RotationMaxVelocity { get; set; } = 50f;
        protected override bool AutoInitialize => false;
        /// <summary>
        /// The amount of time before this <see cref="Particle"/> expires to start fading away. Default: Zero, if <see cref="ExpireTime"/> is null, the <see cref="Particle"/>
        /// will not fade away at all.
        /// </summary>
        public TimeSpan FadeOutTime
        {
            get; set;
        }
        public TimeSpan FadeInTime { get; set; }

        public Particle(Texture2D texture, Vector2 StartPosition, Point? Size = null) : base(texture, StartPosition, Size ?? default)
        {
            if (Size == null)
                this.Size = new Point(texture.Width, texture.Height);
            StartPoint = StartPosition;
            Initialize();
        }

        public Particle(string ParticleKey, Vector2 StartPosition, Point? Size = null) : base("Particles/" + ParticleKey, StartPosition, Size ?? default)
        {
            if (Size == null)
                this.Size = new Point(Texture.Width, Texture.Height);
            StartPoint = StartPosition;
            Initialize();
        }

        public override void Initialize()
        {
            OriginRatio = new Vector2(.5f);
            Expired = false;
            Play();
            base.Color = Color;
            Lifespan = TimeSpan.Zero;
            Position = StartPoint;            
        }

        public override void Update(GameTime gt)
        {
            if (Expired || Paused)
                return;
            Velocity += Acceleration * (float)gt.ElapsedGameTime.TotalSeconds;
            if (Velocity.X > MaxVelocity.X)
                Velocity = new Vector2(MaxVelocity.X, Velocity.Y);
            if (Velocity.Y > MaxVelocity.Y)
                Velocity = new Vector2(Velocity.X, MaxVelocity.Y);
            RotationVelocity += RotationAcceleration * (float)gt.ElapsedGameTime.TotalSeconds;
            if (RotationVelocity > RotationMaxVelocity)
                RotationVelocity = RotationMaxVelocity;
            Rotation += RotationVelocity * (float)gt.ElapsedGameTime.TotalSeconds;
            Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;
            float opacity = 1f;            
            if (ExpireTime != null)
            {
                if (Lifespan > ExpireTime)
                {
                    Visible = false;
                    Expired = true;
                }
                else if (ExpireTime - Lifespan < FadeOutTime)
                {
                    var percent = (ExpireTime - Lifespan).Value.TotalSeconds / FadeOutTime.TotalSeconds;
                    opacity = (float)percent;
                }
                else if (Lifespan < FadeInTime)
                {
                    var percent = (Lifespan.TotalSeconds / FadeInTime.TotalSeconds);
                    opacity = (float)percent;
                }
            }
            if (ZoomFadeMode != ZoomFadeModes.Off)
            {
                float targetOpacity = 0f;
                var provider = ProviderManager.Root.Get<CameraProvider>();
                var cam = provider.Default;
                switch (ZoomFadeMode)
                {
                    case ZoomFadeModes.Appear:
                        targetOpacity = ZoomThreshold > cam.Zoom ? 1f : 0f;
                        break;
                    case ZoomFadeModes.Disappear:
                        targetOpacity = ZoomThreshold > cam.Zoom ? 0f : 1f;
                        break;
                }
                if (ZoomThreshold < cam.Zoom && ZoomThreshold + ZoomFadeRange > cam.Zoom)
                {
                    var percent = cam.Zoom / (ZoomThreshold + ZoomFadeRange);
                    if (targetOpacity == 0f)
                        percent = 1 - percent;
                    opacity *= percent;
                }
                else opacity *= targetOpacity;
            }
            base.Color = StartColor * opacity;
            Lifespan += gt.ElapsedGameTime;
        }      
        
        public void Play()
        {
            Paused = false;
        }
        public void Stop()
        {
            Paused = true;
        }
        public void Restart()
        {
            Stop();
            Initialize();
            Parent.Add(this);
        }
    }
    public class ParticleProvider : IProvider
    {
        private HashSet<Particle> Particles = new HashSet<Particle>();
        public ProviderManager Parent { get; set; }

        public bool Add(Particle p)
        {
            return Particles.Add(p);
        }
        public void Refresh(GameTime gt)
        {
            var hash = new HashSet<Particle>();
            foreach (var Particle in Particles)
            {
                Particle.Update(gt);
                if (Particle.Expired != true)
                    hash.Add(Particle);
            }
            Particles = hash;
        }
        public void Draw(SpriteBatch sb)
        {
            foreach (var Particle in Particles)
                Particle.Draw(sb);
        }
    }
}
