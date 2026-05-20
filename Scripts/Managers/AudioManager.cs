using Godot;
using Godot.Collections;

public partial class AudioManager : Node
{
    public static AudioManager instance { get; private set; }

    public enum MusicType
    {
        Intro,
        InGame,
        Victory,
        End
    }

    public enum SoundType
    {
        Attention,
        Shoot,
        Reload,
        Impact,
        Bump,
        Explosion,
        Time,
        TimeUp,
    }

    [ExportGroup("Audio Streams")]
    [Export] private Array<AudioStream> musicTracks = new Array<AudioStream>();
    [Export] private Array<AudioStream> soundEffects = new Array<AudioStream>();

    private AudioStreamPlayer musicPlayer;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }

        musicPlayer = new AudioStreamPlayer();
        AddChild(musicPlayer);

        musicPlayer.Finished += () => musicPlayer.Play();
    }

    public void PlayMusic(MusicType pType, float pVolumeDb = 0f)
    {
        int lIndex = (int)pType;

        if (lIndex >= 0 && lIndex < musicTracks.Count)
        {
            musicPlayer.Stream = musicTracks[lIndex];
            musicPlayer.VolumeDb = pVolumeDb;
            musicPlayer.Play();
        }
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }

    public AudioStreamPlayer PlaySound(SoundType pType, bool pLoop = false, float pVolumeDb = 5f)
    {
        int lIndex = (int)pType;

        if (lIndex >= 0 && lIndex < soundEffects.Count && soundEffects[lIndex] != null)
        {
            AudioStreamPlayer lSfxPlayer = new AudioStreamPlayer();
            lSfxPlayer.Stream = soundEffects[lIndex];
            lSfxPlayer.VolumeDb = pVolumeDb;
            AddChild(lSfxPlayer);

            if (pLoop)
                lSfxPlayer.Finished += () => lSfxPlayer.Play();
            else
                lSfxPlayer.Finished += () => lSfxPlayer.QueueFree();

            lSfxPlayer.Play();
            return lSfxPlayer;
        }

        return null;
    }

    public void StopSound(AudioStreamPlayer pPlayer)
    {
        pPlayer.Stop();
        pPlayer.QueueFree();
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(pDisposing);
    }
}