using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public sealed class CameraProvider : IProvider
    {
        private Dictionary<string, Camera> cameras = new Dictionary<string, Camera>();
        public Camera Default => cameras.Values.First();
        public ProviderManager Parent { get; set; }

        public CameraProvider()
        {
            Create(); // creates the root camera
        }

        public Camera Get(string key)
        {
            if (cameras.TryGetValue(key, out var camera))
                return camera;
            else
                return null;            
        }

        public Camera Create(string name = "root") => Add(new Camera() { Name = name });

        public Camera Add(Camera cam)
        {
            if (Get(cam.Name) == null)
                cameras.Add(cam.Name, cam);
            return cam;
        }

        public void Refresh(GameTime gameTime)
        {
            foreach (var cam in cameras.Values)
                cam.Update(gameTime);
        }
    }
}
