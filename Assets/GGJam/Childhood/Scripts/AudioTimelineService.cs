using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Сервис для воспроизведения аудио на определенных временных метках с общей очередью
/// </summary>
public class AudioTimelineService : MonoBehaviour
{
    [System.Serializable]
    public class TimeMarker
    {
        public float timeInSeconds;
        [HideInInspector] public bool triggered = false;
    }

    [System.Serializable]
    public class QueuedAudioClip
    {
        public AudioClip clip;
        public float volume;
        public int offsetMs;
    }

    [Header("Main Audio")]
    public AudioSource mainAudioSource;
    public AudioClip mainTrack;

    [Header("Time Markers")]
    public List<TimeMarker> timeMarkers = new List<TimeMarker>();

    [Header("Settings")]
    public bool autoStart = true;
    public bool usePooling = true;
    public int poolSize = 8;
    public bool debugMode = false;

    // Общая очередь для всех клипов
    private Queue<QueuedAudioClip> clipQueue = new Queue<QueuedAudioClip>();

    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private float startTime;
    private int lastTriggeredMarkerIndex = -1;
    private float currentPlaybackTime = 0f;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeService();
    }

    private void Start()
    {
        if (autoStart && mainTrack != null)
        {
            PlayMainTrack();
        }
    }

    private void Update()
    {
        if (!mainAudioSource.isPlaying) return;
        currentPlaybackTime = GetCurrentPlaybackTime();
        CheckTimeMarkers();

    }

    #endregion

    #region Initialization

    private void InitializeService()
    {
        // Инициализируем основной аудиоисточник
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

        // Создаем пул аудиоисточников для эффективного использования
        if (usePooling)
        {
            CreateAudioSourcePool();
        }

        // Сортируем маркеры по времени
        SortMarkersByTime();
    }

    private void CreateAudioSourcePool()
    {
        // Очищаем существующие источники, если они есть
        foreach (var source in audioSourcePool)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }
        
        audioSourcePool.Clear();

        // Создаем новый пул
        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObj = new GameObject($"AudioPool_{i}");
            audioObj.transform.parent = transform;
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Add(source);
        }
        
        LogDebug($"Created audio source pool with {poolSize} sources");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Начать воспроизведение основного трека
    /// </summary>
    public void PlayMainTrack()
    {
        if (mainAudioSource.clip == null)
        {
            Debug.LogWarning("No main track assigned to play");
            return;
        }

        mainAudioSource.Play();
        mainAudioSource.loop = true;
        startTime = Time.time;
        
        // Сбрасываем состояние маркеров
        ResetMarkers();
        lastTriggeredMarkerIndex = -1;
        
        LogDebug("Started playing main track");
    }

    /// <summary>
    /// Пауза воспроизведения
    /// </summary>
    public void PausePlayback()
    {
        if (mainAudioSource.isPlaying)
        {
            mainAudioSource.Pause();
            LogDebug("Paused playback");
        }
    }

    /// <summary>
    /// Продолжить воспроизведение после паузы
    /// </summary>
    public void ResumePlayback()
    {
        if (!mainAudioSource.isPlaying && mainAudioSource.clip != null)
        {
            mainAudioSource.UnPause();
            // Корректируем startTime, чтобы учесть время, которое уже прошло
            startTime = Time.time - mainAudioSource.time;
            LogDebug("Resumed playback");
        }
    }

    /// <summary>
    /// Остановить воспроизведение
    /// </summary>
    public void StopPlayback()
    {
        mainAudioSource.Stop();
        ResetMarkers();
        LogDebug("Stopped playback");
    }

    /// <summary>
    /// Добавить аудио клип в очередь с отступом в миллисекундах
    /// </summary>
    /// <param name="clip">Аудио клип для воспроизведения</param>
    /// <param name="offsetMs">Отступ в миллисекундах от времени следующего маркера</param>
    /// <param name="volume">Громкость (0.0-1.0)</param>
    public void EnqueueClip(AudioClip clip, int offsetMs = 0, float volume = 1.0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Trying to enqueue null audio clip");
            return;
        }
        
        QueuedAudioClip queuedClip = new QueuedAudioClip
        {
            clip = clip,
            volume = volume,
            offsetMs = offsetMs
        };
        
        clipQueue.Enqueue(queuedClip);
        
        // Находим следующий маркер для отображения в логах
        string nextMarkerInfo = GetNextMarkerInfo();
        
        LogDebug($"Enqueued clip {clip.name} with offset {offsetMs}ms " +
                 $"(queue size: {clipQueue.Count}) - Next: {nextMarkerInfo}");
    }

    /// <summary>
    /// Очистить всю очередь клипов
    /// </summary>
    public void ClearQueue()
    {
        clipQueue.Clear();
        LogDebug("Cleared audio queue");
    }

    /// <summary>
    /// Создать новый маркер программно
    /// </summary>
    /// <param name="markerName">Имя нового маркера</param>
    /// <param name="timeInSeconds">Время в секундах</param>
    public void CreateMarker(string markerName, float timeInSeconds)
    {
       
        TimeMarker newMarker = new TimeMarker
        {
            timeInSeconds = timeInSeconds,
            triggered = false
        };
        
        timeMarkers.Add(newMarker);
        SortMarkersByTime();
        
        LogDebug($"Created new marker {markerName} at {timeInSeconds} seconds");
    }

    /// <summary>
    /// Получить текущее время воспроизведения основного трека
    /// </summary>
    public float GetCurrentPlaybackTime()
    {
        if (mainAudioSource == null || mainAudioSource.clip == null)
            return 0f;
            
        return mainAudioSource.time;
    }

    /// <summary>
    /// Переход к определенному времени в треке
    /// </summary>
    public void SeekToTime(float timeInSeconds)
    {
        if (mainAudioSource.clip != null)
        {
            mainAudioSource.time = Mathf.Clamp(timeInSeconds, 0, mainAudioSource.clip.length);
            startTime = Time.time - mainAudioSource.time;
            
            // Сбрасываем маркеры и находим текущий
            ResetMarkers();
            UpdateMarkerStateBasedOnTime(mainAudioSource.time);
            
            LogDebug($"Seeked to time: {timeInSeconds} seconds");
        }
    }

    /// <summary>
    /// Получить количество клипов в очереди
    /// </summary>
    public int GetQueueCount()
    {
        return clipQueue.Count;
    }

    /// <summary>
    /// Получить информацию о следующем маркере
    /// </summary>
    public string GetNextMarkerInfo()
    {
        int nextIndex = GetNextMarkerIndex();
        if (nextIndex >= 0 && nextIndex < timeMarkers.Count)
        {
            var marker = timeMarkers[nextIndex];
            return $"'marker at {marker.timeInSeconds:F2}s";
        }
        
        return "No upcoming markers";
    }

    #endregion

    #region Internal Methods

    private void ResetMarkers()
    {
        foreach (var marker in timeMarkers)
        {
            marker.triggered = false;
        }
    }

    private void UpdateMarkerStateBasedOnTime(float currentTime)
    {
        lastTriggeredMarkerIndex = -1;
        
        for (int i = 0; i < timeMarkers.Count; i++)
        {
            if (timeMarkers[i].timeInSeconds <= currentTime)
            {
                timeMarkers[i].triggered = true;
                lastTriggeredMarkerIndex = i;
            }
        }
    }

    private int GetNextMarkerIndex()
    {
        // Определяем индекс следующего маркера
        int nextIndex = lastTriggeredMarkerIndex + 1;
        
        if (nextIndex >= timeMarkers.Count)
        {
            // Нет больше маркеров
            return -1;
        }
        
        return nextIndex;
    }

    private void CheckTimeMarkers()
    {
        if (timeMarkers.Count == 0 || clipQueue.Count == 0)
            return;
        
        int nextMarkerIndex = GetNextMarkerIndex();
        if (nextMarkerIndex == -1) return; // Нет следующего маркера
        
        TimeMarker nextMarker = timeMarkers[nextMarkerIndex];
        
        // Если достигли времени следующего маркера и есть клипы в очереди
        if (Math.Abs(currentPlaybackTime - nextMarker.timeInSeconds) < 0.050 && !nextMarker.triggered)
        {
            nextMarker.triggered = true;
            lastTriggeredMarkerIndex = nextMarkerIndex;
            
            LogDebug($"Triggered marker at {currentPlaybackTime:F2}s");
            
            // Воспроизводим все доступные клипы в очереди
            PlayQueuedClips(nextMarker.timeInSeconds);
        }
    }

    private void PlayQueuedClips(float markerTime)
    {
            
        QueuedAudioClip queuedClip = clipQueue.Dequeue();
            
        // Вычисляем абсолютное время воспроизведения с учетом отступа
        float offsetInSeconds = queuedClip.offsetMs / 1000.0f;
        float absolutePlayTime = markerTime + offsetInSeconds;
            
        float delayTime = absolutePlayTime - currentPlaybackTime;
        StartCoroutine(PlayClipWithDelay(queuedClip.clip, queuedClip.volume, delayTime));
        LogDebug($"Will play {queuedClip.clip.name} after {delayTime:F2}s delay (offset {queuedClip.offsetMs}ms)");
        

    }

    private IEnumerator PlayClipWithDelay(AudioClip clip, float volume, float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);
        PlayClip(clip, volume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null) return;

        AudioSource source;
        
        if (usePooling)
        {
            source = GetAvailableAudioSource();
        }
        else
        {
            // Создаём временный аудиоисточник
            GameObject tempGO = new GameObject("TempAudio");
            source = tempGO.AddComponent<AudioSource>();
            Destroy(tempGO, clip.length + 0.5f); // Небольшой запас времени
        }

        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        // Запускаем корутину, которая отметит, когда этот источник освободится
        StartCoroutine(TrackAudioSourceCompletion(source, clip.length));
    }

    private IEnumerator TrackAudioSourceCompletion(AudioSource source, float clipLength)
    {
        yield return new WaitForSeconds(clipLength + 0.1f);
        
        // Сброс источника для повторного использования
        if (source != null && audioSourcePool.Contains(source))
        {
            source.clip = null;
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Сначала ищем неиспользуемый источник
        foreach (var source in audioSourcePool)
        {
            if (source != null && !source.isPlaying)
            {
                return source;
            }
        }
        
        // Если все заняты, ищем тот, который скоро закончится
        AudioSource bestCandidate = audioSourcePool[0]; // Берем первый как запасной вариант
        float shortestRemainingTime = float.MaxValue;
        
        foreach (var source in audioSourcePool)
        {
            if (source != null && source.clip != null)
            {
                float remainingTime = source.clip.length - source.time;
                if (remainingTime < shortestRemainingTime)
                {
                    shortestRemainingTime = remainingTime;
                    bestCandidate = source;
                }
            }
        }
        
        // Останавливаем текущее воспроизведение и возвращаем этот источник
        if (bestCandidate != null)
        {
            bestCandidate.Stop();
        }
        
        return bestCandidate;
    }

    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[AudioTimelineService] {message}");
        }
    }

    #endregion

    #region Editor Methods

    // Метод для редактора Unity - добавляет пустой маркер
    [ContextMenu("Add Empty Marker")]
    public void AddEmptyMarker()
    {
        TimeMarker newMarker = new TimeMarker
        {
            timeInSeconds = 0f,
            triggered = false
        };
        
        timeMarkers.Add(newMarker);
        SortMarkersByTime();
    }

    // Сортировка маркеров по времени (для инспектора)
    [ContextMenu("Sort Markers by Time")]
    public void SortMarkersByTime()
    {
        timeMarkers.Sort((a, b) => a.timeInSeconds.CompareTo(b.timeInSeconds));
    }

    #endregion
}