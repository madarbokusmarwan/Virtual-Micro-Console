using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMicroConsole
{
    public class SoundManager
    {
        private SoundEffect[] Sounds;
        private Song[] Musics;

        public SoundManager(ContentManager content)
        {
            Sounds = new SoundEffect[24];
            for (int i = 0; i < 24; i++) Sounds[i] = content.Load<SoundEffect>("assets/sounds/sound" + i);
            Musics = new Song[9];
            for (int i = 0; i < 9; i++) Musics[i] = content.Load<Song>("assets/musics/music" + i);
            MediaPlayer.Volume = 0.15f;
        }
        public void PlaySound(int id)
        {
            Sounds[id].Play();
        }
        public void PlayMusic(int id, bool loop = false)
        {
            MediaPlayer.Play(Musics[id]);
            MediaPlayer.IsRepeating = loop;
        }
        public void Pause()
        {
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause();
            else MediaPlayer.Resume();
        }
        public void Stop()
        {
            MediaPlayer.Stop();
        }

        public Song GetMusic(int id)
        {
            return Musics[id];
        }
        public SoundEffect GetSound(int id)
        {
            return Sounds[id];
        }
    }
}
