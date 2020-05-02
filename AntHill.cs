using AntFarm.Common.Engine;
using AntFarm.Common.Provider;
using AntFarm.Common.Provider.Input;
using AntFarm.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AntFarm.Common.Provider.Input.InputProvider;

namespace AntFarm.Common
{
    public class AntHill : GameObject, IAntSpace, ITileObject, IClickable
    {
        const int WIDTH = 120, HEIGHT = 240;
        List<Ant> insideAnts;
        private Ant _queen;
        public World CurrentWorld
        {
            get; private set;
        }

        public int RestingAnts => insideAnts.Count;
        public Ant Queen
        {
            get => _queen;
            set
            {
                _queen = value;
                Enter(value);
            }
        }
        public WorldTile Parent
        {
            get; set;
        }
        public Vector2 Offset { get; set; }
        public GameObject ThisObject => this;
        public bool IsMouseOver { set; get; }

        public AntHill(World world, Vector2 Position) : base("Objects/hill_poor", Position, new Point(WIDTH, HEIGHT))
        {
            Offset = new Vector2(0, (WorldTile.TILE_HEIGHT / 2) - HEIGHT);
            CurrentWorld = world;
        }

        public override void Initialize()
        {
            insideAnts = new List<Ant>();
            ProviderManager.Root.Get<InputProvider>().Subscribe(this);
        }        

        public void Enter(Ant ant)
        {
            if (ant.CurrentSpace != this)
            {
                insideAnts.Add(ant);
                ant.CurrentSpace = this;
                ant.Position = Parent.Center.ToVector2();
                ant.Visible = false;
                if (Queen == null)
                    Queen = ant;                        
            }
        }    
        
        public void ExitToWorld(Ant ant)
        {
            CurrentWorld.Enter(ant);
        }

        public override void Update(GameTime gt)
        {
            foreach (var ant in insideAnts)
                ant.Update(gt);
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            if (IsMouseOver)
                GameResources.DrawMouseHighlight(batch, this);
        }

        public void MouseEnter(GameTime time)
        {
            
        }

        public void Clicked(GameTime time, InputEventArgs args)
        {
            
        }

        public void MouseDown(GameTime time)
        {
            
        }
    }
}
