using AntFarm.Common.Actions;
using AntFarm.Common.Engine;
using AntFarm.Common.Provider;
using AntFarm.Common.Provider.Input;
using AntFarm.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public class FoodItem : Item
    {
        public int Worth { get; private set; }
        public FoodItem(IItemHolder parent, int worth, Color color) : base(
            ProviderManager.Root.Get<LazerContentManager>().GetTexture("Objects/FoodItem"), new Point(20), parent)
        {
            Worth = worth;
            Color = color;
        }

        public override void Initialize()
        {
            
        }
    }
    public class Food : GameObject, ITileObject, IClickable, IActionInteractable<Ant>
    {
        public enum FoodType : int
        {
            Apple = 50,
            Watermelon = 75
        }

        const int HEIGHT = 75;
        public int Value
        {
            get; set;
        } = (int)FoodType.Apple;

        public FoodType Type
        {
            get;private set;
        }
        public int MaxValue => (int)Type;
        public WorldTile Parent { get; set; }

        public GameObject ThisObject => this;

        public Vector2 Offset { get; set; }

        public bool IsMouseOver { get; set; }

        public Food(Vector2 Position) : base("Objects/apple", Position, new Point(HEIGHT))
        {
            Offset = new Vector2(-5, 40);
            var foodType = GameResources.Rand.Next(0, 2);
            switch (foodType)
            {
                case 0:
                    Type = FoodType.Apple;
                    break;
                case 1:
                    Type = FoodType.Watermelon;
                    break;
            }
            Value = (int)Type;
            Texture = GetTextureByValue();            
        }
       
        public override void Initialize()
        {
            ProviderManager.Root.Get<InputProvider>().Subscribe(this);
        }

        public bool Harvest(IItemHolder holder, int amount = 1)
        {
            if (Value <= 0)
                return false;
            var foodItem = new FoodItem(holder, amount, Color.LimeGreen);
            holder.Equip(foodItem);
            Value -= amount;
            Texture = GetTextureByValue();
            return true;
        }

        private Texture2D GetTextureByValue()
        {
            var contentProvider = ProviderManager.Root.Get<LazerContentManager>();
            Texture2D Texture = default(Texture2D);
            var name = "watermelon";
            switch (Type)
            {
                case FoodType.Apple:
                    name = "apple";
                    break;
            }
            if (Value > (int)(MaxValue * (3.0 / 4.0))) // value is greater than 3/4            
                Texture = contentProvider.GetTexture($"Objects/{name}");
            else if (Value > (int)(MaxValue / 2)) // greater 1/2
                Texture = contentProvider.GetTexture($"Objects/{name}_34");
            else if (Value > (int)(MaxValue * (0))) // greater than 1/4
                Texture = contentProvider.GetTexture($"Objects/{name}_half");
            else Texture = contentProvider.GetTexture($"Objects/{name}_14");
            return Texture;
        }

        public override void Update(GameTime gt)
        {
               
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

        public void MouseDown(GameTime time)
        {
            
        }

        public void Clicked(GameTime time, InputProvider.InputEventArgs args)
        {
            var mode = Mouse.GetState().LeftButton == ButtonState.Pressed;
            if (mode) //left button
            {
                
            }
            else //right button
            {
                var provider = ProviderManager.Root.Get<ActionGroupProvider<Ant>>();
                if (provider.CurrentGroup == null)
                    provider.SetSelectedGroup(ProviderManager.Root.Get<WorldProvider>().Current.GetAllAvailableAnts());
                Interact(provider.CurrentGroup);
            }
        }

        public void Interact(ActionObjectGroup<Ant> Group)
        {            
            var leader = Group.Items.FirstOrDefault();
            var content = ProviderManager.Root.Get<LazerContentManager>();
            var safezone = content.GetTextureSafezone(TextureFileName);
            Group.EnqueueActionSequentially(new AntRoutingAction(
                    leader?.Position ?? new Vector2(),
                    safezone.Value.Location.ToVector2() + Position), .25);
            Group.EnqueueAction(new AntRoutingAction(this));
            Group.EnqueueAction(new AntRepetitiveAction(
                1, 1, AntRepetitiveAction.RepeatMode.Iterative,
                (Ant ant, AntRepetitiveAction action, GameTime gt) =>
                {
                    Harvest(ant);
                }));
            safezone = content.GetTextureSafezone(leader.Home.TextureFileName);
            Group.EnqueueActionSequentially(new AntRoutingAction(
                    leader?.Position ?? new Vector2(),
                    safezone.Value.Location.ToVector2() + leader.Home.Position), .25);
            Group.EnqueueAction(new AntRoutingAction(leader.Home));
            Group.EnqueueAction(new AntRepetitiveAction(
               1, 1, AntRepetitiveAction.RepeatMode.Iterative,
               (Ant ant, AntRepetitiveAction action, GameTime gt) =>
               {
                   ant.DropAll();
               }));
        }
    }
}
