using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioTimeManager : MonoBehaviour
{
    [System.Serializable]
    public class TimedAudioClip
    {
        public AudioClip clip;
        public float playAtTime; // время в секундах, когда нужно воспроизвести клип
        public float volume = 1.0f;
        public bool hasPlayed = false;
    }

    [Header("Main Audio")]
    public AudioSource mainAudioSource; // основной аудиоисточник
    public AudioClip mainTrack; // основной трек

    [Header("Timed Clips")]
    public List<TimedAudioClip> timedClips = new List<TimedAudioClip>(); // список клипов с временными метками
    
    [Header("Settings")]
    public bool autoStart = true;
    public bool usePooling = true;
    public int poolSize = 5;

    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private float startTime;
    private bool isPlaying = false;

    private void Start()
    {
        // Инициализируем основной аудиоисточник, если он не был назначен
        if (mainAudioSource == null)
        {
            mainAudioSource = GetComponent<AudioSource>();
            if (mainAudioSource == null)
            {
                mainAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Назначаем основной трек
        if (mainTrack != null)
        {
            mainAudioSource.clip = mainTrack;
        }

        // Если используем пулинг, создаём дополнительные аудиоисточники
        if (usePooling)
        {
            CreateAudioSourcePool();
        }

        // Если включен автозапуск, начинаем воспроизведение
        if (autoStart && mainAudioSource.clip != null)
        {
            StartPlayback();
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        float currentTime = Time.time - startTime;

        // Проверяем все тайминги для воспроизведения дополнительных клипов
        foreach (TimedAudioClip timedClip in timedClips)
        {
            if (!timedClip.hasPlayed && currentTime >= timedClip.playAtTime)
            {
                PlayClipAtTime(timedClip);
                timedClip.hasPlayed = true;
            }
        }

        // Если основной трек закончился, сбрасываем всё
        if (!mainAudioSource.isPlaying && isPlaying)
        {
            ResetPlayback();
        }
    }

    public void StartPlayback()
    {
        if (mainAudioSource.clip == null) return;

        mainAudioSource.Play();
        startTime = Time.time;
        isPlaying = true;
        
        // Сбрасываем флаги воспроизведения для всех клипов
        foreach (TimedAudioClip timedClip in timedClips)
        {
            timedClip.hasPlayed = false;
        }
    }

    public void PausePlayback()
    {
        if (isPlaying)
        {
            mainAudioSource.Pause();
            isPlaying = false;
        }
    }

    public void ResumePlayback()
    {
        if (!isPlaying && mainAudioSource.clip != null)
        {
            mainAudioSource.UnPause();
            startTime = Time.time - mainAudioSource.time;
            isPlaying = true;
        }
    }

    public void StopPlayback()
    {
        mainAudioSource.Stop();
        isPlaying = false;
    }

    public void ResetPlayback()
    {
        StopPlayback();
        
        foreach (TimedAudioClip timedClip in timedClips)
        {
            timedClip.hasPlayed = false;
        }
    }

    private void PlayClipAtTime(TimedAudioClip timedClip)
    {
        if (timedClip.clip == null) return;

        AudioSource source;
        
        if (usePooling)
        {
            // Находим неиспользуемый аудиоисточник из пула
            source = GetAvailableAudioSource();
        }
        else
        {
            // Создаём временный аудиоисточник
            GameObject tempGO = new GameObject("TempAudio");
            source = tempGO.AddComponent<AudioSource>();
            Destroy(tempGO, timedClip.clip.length);
        }

        // Воспроизводим нужный клип
        source.clip = timedClip.clip;
        source.volume = timedClip.volume;
        source.Play();
    }

    private void CreateAudioSourcePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObj = new GameObject("AudioPool_" + i);
            audioObj.transform.parent = transform;
            AudioSource source = audioObj.AddComponent<AudioSource>();
            audioSourcePool.Add(source);
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // Если все аудиоисточники заняты, используем первый
        return audioSourcePool[0];
    }

    // Метод для добавления нового клипа с временной меткой
    public void AddTimedClip(AudioClip clip, float time, float volume = 1.0f)
    {
        TimedAudioClip newClip = new TimedAudioClip
        {
            clip = clip,
            playAtTime = time,
            volume = volume,
            hasPlayed = false
        };
        
        timedClips.Add(newClip);
    }
}