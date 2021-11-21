using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public class AudioProvider : IProvider
    {
        public float Volume
        {
            get; set;
        } = 1;

        public ProviderManager Parent { get; set; }
        private List<SoundEffectInstance> _soundEffects = new List<SoundEffectInstance>();
        private SoundEffectInstance music;

        public bool PlaySoundEffect(string Sound) => 
            PlaySoundEffect(
                ProviderManager.Root.Get<ContentProvider>().GetSoundEffect("SFX/" + Sound));
        public bool PlaySoundEffect(SoundEffect Effect)
        {
            var effect = Effect.CreateInstance();
            effect.Volume = Volume;
            try
            {
                effect.Play();
            }
            catch(InstancePlayLimitException e)
            {
                return false;
            }
            _soundEffects.Add(effect);
            return true;
        }

        public void StopMusic()
        {
            if (music == null) return; // NO MUSIC PLAYING
            music.Stop();
            music.Dispose();
        }

        public SoundEffectInstance PlayMusic(string Sound)
        {
            StopMusic();
            var song = ProviderManager.Root.Get<ContentProvider>().GetSoundEffect("Music/" + Sound);
            if (song == null) return null;
            var instance = song.CreateInstance();
            instance.IsLooped = true;
            instance.Volume = Volume;
            instance.Play();
            return music = instance;
        }

        public void Refresh(GameTime time)
        {
            for(int i = 0; i < _soundEffects.Count; i++)
            {
                var effect = _soundEffects[i];
                if (effect != null)
                    effect.Volume = Volume;
                if (effect.State == SoundState.Stopped)
                {
                    effect.Dispose();
                    _soundEffects.Remove(effect);
                }
            }
        }
    }
}
