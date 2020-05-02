using AntFarm.Common.Actions;
using AntFarm.Common.Engine;
using AntFarm.Common.Provider;
using AntFarm.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public class Ant : GameObject, IActionRunnable, IItemHolder
    {        
        public const double SPEED_SECONDSPERTILE = 1.0 / WorldTile.TILE_WIDTH;

        private Chromosome Genetics;
        public AntHill Home
        {
            get; private set;
        }
        public Chromosome.PersonalityTraits Personality
        {
            get;
            private set;
        }

        public Queue<AntAction> ActionQueue
        {
            get;
        } = new Queue<AntAction>();
        public ActionObjectGroup<IActionRunnable> ParentGroup { get; set; }
        public HashSet<Item> HoldingItems { get; set; } = new HashSet<Item>();

        public IAntSpace CurrentSpace;        

        public Ant(Vector2 Position, AntHill home, Chromosome chromosome) : base("Ants/ant_SW", Position, new Point(25))
        {            
            Genetics = chromosome;
            Personality = Genetics.GetExhibitedTrait();
            Home = home;
            home.Enter(this);
            updateGraphic(Direction.N);
        }

        public Ant(Vector2 Position, AntHill home, Ant Parent1, Ant Parent2) : 
            this(Position, home, Chromosome.Procreate(Parent1.Genetics, Parent2.Genetics))
        {

        }

        public T EnqueueAction<T>(T action) where T : AntAction
        {
            ActionQueue.Enqueue(action);
            action.Subject = this;
            action.OnActionCompleted += OnActionCompleted;
            return action;
        }

        private void OnActionCompleted(Ant subject, AntAction completed)
        {
            if (ActionQueue.Peek() == completed)
                ActionQueue.Dequeue();
        }

        public void SetDestination(ITileObject destination)
        {
            var routingAction = EnqueueAction(new AntRoutingAction(this, destination));
            var direction = routingAction.GetTravelAngle();
            updateGraphic(routingAction.GetTravelDirection(direction));
            routingAction.OnRoutingCompleted += (Ant subject, ITileObject obj) =>
            { // anonymous routing event completed

            };
        }

        public override void Initialize()
        {
            
        }

        private void Update_InsideHill(GameTime gt)
        {
            Home.ExitToWorld(this);
            Position = GameResources.GetRandomVector2(Home.Position.X - 10, Home.Position.X + 10,
                Home.Position.Y - 10, Home.Position.Y + 10);
        }        

        private void updateGraphic(Direction direction)
        {
            var dirStr = Enum.GetName(typeof(Direction), direction);
            var assetName = "Ants/ant_" + direction;
            Texture = ProviderManager.Root.Get<LazerContentManager>().GetTexture(assetName);
            Size = new Point(Texture.Width, Texture.Height);
        }

        private void Update_AboveGround(GameTime gt)
        {
            if (ActionQueue.Any())
                ActionQueue.Peek().Run(gt);
            foreach (var holdingobj in HoldingItems)
                holdingobj.Update(gt);
        }

        public override void Update(GameTime gt)
        {
            if (CurrentSpace is World)
                Update_AboveGround(gt);
            else if (CurrentSpace == Home)
                Update_InsideHill(gt);
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            foreach (var obj in HoldingItems)
                obj.Draw(batch);
        }

        public Item Equip(Item item)
        {
            HoldingItems.Add(item);
            return item;
        }

        public bool Drop(Item item)
        {
            return HoldingItems.Remove(item);
        }

        public bool DropAll()
        {
            HoldingItems.Clear();
            return true;
        }
    }
}
