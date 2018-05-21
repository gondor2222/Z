using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {
    private enum Mode { MAIN, PAUSE, MAP, CREDITS };
    private string text;
    private Mode previousMode;
    class Mixer
    {
        public AudioSource player;
        public Song[] songs;
        Music parentMusic;
        int index;
        int nextIndex;
        public ResourceRequest nextLoad;

        public Mixer(AudioSource player, Song[] songs, Music parentMusic)
        {
            this.player = player;
            this.songs = songs;
            this.parentMusic = parentMusic;
            nextIndex = Random.Range(0, songs.Length);
            nextLoad = Resources.LoadAsync(songs[nextIndex].path, typeof(AudioClip));
        }

        public void StartNext()
        {
            if (player.isPlaying)
            {
                Debug.Log("Erroneously called player to start new song.");
                return;
            }
            index = nextIndex;
            nextIndex = (index + Random.Range(1, songs.Length)) % songs.Length;
            if (!songs[index].isLoaded)
            {
                if (!nextLoad.isDone)
                {
                    Debug.Log("Next song wasn't loaded.");
                }
                if (nextLoad.asset == null)
                {
                    Debug.Log("WARNING: ATTEMPT TO LOAD SONG RESULTED IN NULL ASSET");
                    Debug.Log(string.Format("AT {0}", songs[index].name));
                }
                songs[index].audio = nextLoad.asset as AudioClip;
                songs[index].isLoaded = true;
            }
            if (!songs[nextIndex].isLoaded) {
                nextLoad = Resources.LoadAsync(songs[nextIndex].path, typeof(AudioClip));
            }

            player.clip = songs[index].audio;
            Debug.Log(string.Format("Loaded {0}", songs[index].name));
            parentMusic.text = string.Format("Now playing \n{0}\nby {1}", songs[index].name, songs[index].author);
            player.Play();
        }

        public void UnPause()
        {
            player.UnPause();
            string newText = string.Format("Now playing \n{0}\nby {1}", songs[index].name, songs[index].author);
            parentMusic.text = newText;
        }
    }
    class Song
    {
        public AudioClip audio;
        public string path;
        public string name;
        public string author;
        public bool isLoaded;
        public Song(string path, string name, string author)
        {
            isLoaded = false;
            this.path = path;
            this.name = name;
            this.author = author;
        }
    }
    static readonly Song[] mainSongs = new Song[] {
            new Song("Audio/Music/Main/Magna_Ingress_-_07_-_Imaginary_Sunshine", "Imaginary Sunshine", "Magna Ingress"),
            new Song("Audio/Music/Main/Jellyfish In Space", "JellyFish In Space", "Kevin MacLeod"),
            new Song("Audio/Music/Main/Infinite Ocean", "Infinite Ocean", "Purple Planet"), //During low mass segment?
            new Song("Audio/Music/Main/Kai_Engel_-_08_-_Chance", "Chance", "Kai Engel"),
            new Song("Audio/Music/Main/Tranquilize", "Tranquilize", "Purple Planet")
        };
    static readonly Song[] pauseSongs = new Song[] {
            new Song("Audio/Music/Pause/Shifting Sands", "Shifting Sands", "Purple Planet"),
            new Song("Audio/Music/Pause/Troposphere", "Troposphere", "Purple Planet"),
            new Song("Audio/Music/Pause/Remember the Stars", "Remember the Stars - Techno", "Dan O'Connor (IX)"),
            new Song("Audio/Music/Pause/Magna_Ingress_-_11_-_The_Glow", "The Glow", "Magna Ingress")
        };
    static readonly Song[] mapSongs = new Song[] {
            new Song("Audio/Music/Map/bensound-straight", "Straight", "Bensound"),
            new Song("Audio/Music/Map/bensound-scifi", "Sci Fi", "Bensound"),
            new Song("Audio/Music/Map/Soaring", "Soaring", "Kevin MacLeod"), //Main Menu
            new Song("Audio/Music/Map/bensound-relaxing", "Relaxing", "Bensound"),
            new Song("Audio/Music/Map/Copper Mountain", "Copper Mountain", "Dan O'Connor (IX)")
     };
    static readonly Song[] creditsSongs = new Song[] {
        new Song("Audio/Music/Other/Sergey_Cheremisinov_-_04_-_Glow_in_Space", "Glow in Space", "Sergey Cheremisinov")
    };
    Mode mode;
    private Mixer mainMixer;
    private Mixer pauseMixer;
    private Mixer mapMixer;
    private Mixer creditsMixer;
    GUIStyle style;
    void Awake()
    {
        mainMixer = new Mixer(GameObject.Find("MainAudio").GetComponent<AudioSource>(), mainSongs, GameObject.Find("Daemon").GetComponent<Music>());
        pauseMixer = new Mixer(GameObject.Find("PauseAudio").GetComponent<AudioSource>(), pauseSongs, GameObject.Find("Daemon").GetComponent<Music>());
        mapMixer = new Mixer(GameObject.Find("MapAudio").GetComponent<AudioSource>(), mapSongs, GameObject.Find("Daemon").GetComponent<Music>());
        creditsMixer = new Mixer(GameObject.Find("PauseAudio").GetComponent<AudioSource>(), creditsSongs, GameObject.Find("Daemon").GetComponent<Music>());
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        mode = Mode.MAIN;
        style = new GUIStyle();
        style.font = Resources.Load("Fonts/cour", typeof(Font)) as Font;
        style.fontSize = 24;
        style.normal.textColor = new Color(1, 1, 1, 0.1f);
        style.alignment = TextAnchor.MiddleLeft;

        watch.Stop();
        Debug.Log(string.Format("Took {0} ms to initialize audio.\n", watch.ElapsedMilliseconds));

    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (mode == Mode.MAIN && !mainMixer.player.isPlaying)
        {
            Debug.Log("Playing new main song.");
            mainMixer.StartNext();
            Debug.Log("Finished starting new main song.");
        }
        else if (mode == Mode.PAUSE && !pauseMixer.player.isPlaying)
        {
            Debug.Log("Playing new pause song.");
            pauseMixer.StartNext();
        }
        else if (mode == Mode.MAP && !mapMixer.player.isPlaying)
        {
            Debug.Log("Playing new map song.");
            mapMixer.StartNext();
        }
        else if (mode == Mode.CREDITS && !creditsMixer.player.isPlaying)
        {
            Debug.Log("Playing new credits song.");
            creditsMixer.StartNext();
        }
	}
    public void SwitchToMap()
    {
        previousMode = Mode.MAP;
    }

    public void SwitchToMain()
    {
        previousMode = Mode.MAIN;
    }
    public void Pause()
    {
        previousMode = mode;
        mode = Mode.PAUSE;
        mapMixer.player.Pause();
        mainMixer.player.Pause();
        pauseMixer.UnPause();
    }

    public void SwitchToCredits()
    {
        pauseMixer.player.Pause();
        mode = Mode.CREDITS;
    }

    public void StopCredits()
    {
        creditsMixer.player.Pause();
        mode = Mode.PAUSE;
    }

    public void UnPause()
    {
        pauseMixer.player.Pause();
        if (previousMode == Mode.MAP)
        {
            mapMixer.UnPause();
            mode = Mode.MAP;
        }
        else if (previousMode == Mode.MAIN)
        {
            mainMixer.UnPause();
            mode = Mode.MAIN;
        }
        else
        {
            Debug.Log("Error: Mode before pause was pause?");
        }
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 70, 500, 50), text, style);
    }
}
