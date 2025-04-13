using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///     Сервис для воспроизведения аудио на определенных временных метках с общей очередью
/// </summary>
public class AudioTimelineService : MonoBehaviour
{
	public event Action<Note> NotePlay;

	[System.Serializable]
	public class TimeMarker
	{
		public float timeInSeconds;
		[HideInInspector]
		public bool triggered = false;
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
	private float currentPlaybackTime = 0f;
	private float startTime;

	private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();

	// Общая очередь для всех клипов
	private readonly Queue<Note> clipQueue = new Queue<Note>();

	#region Unity Lifecycle

	private void Awake()
	{
		InitializeService();
	}

	private void Start()
	{
		if (autoStart && mainTrack != null)
			PlayMainTrack();
	}

	private void Update()
	{
		if (!mainAudioSource.isPlaying)
			return;

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
				mainAudioSource = gameObject.AddComponent<AudioSource>();
		}

		// Назначаем основной трек
		if (mainTrack != null)
			mainAudioSource.clip = mainTrack;

		// Создаем пул аудиоисточников для эффективного использования
		if (usePooling)
			CreateAudioSourcePool();

		// Сортируем маркеры по времени
		SortMarkersByTime();
	}

	private void CreateAudioSourcePool()
	{
		// Очищаем существующие источники, если они есть
		foreach (var source in audioSourcePool)
			if (source != null)
				Destroy(source.gameObject);

		audioSourcePool.Clear();

		// Создаем новый пул
		for (var i = 0; i < poolSize; i++)
		{
			var audioObj = new GameObject($"AudioPool_{i}");
			audioObj.transform.parent = transform;
			var source = audioObj.AddComponent<AudioSource>();
			source.playOnAwake = false;
			audioSourcePool.Add(source);
		}

		LogDebug($"Created audio source pool with {poolSize} sources");
	}

	#endregion

	#region Public API

	/// <summary>
	///     Начать воспроизведение основного трека
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

		LogDebug("Started playing main track");
	}

	/// <summary>
	///     Пауза воспроизведения
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
	///     Продолжить воспроизведение после паузы
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
	///     Остановить воспроизведение
	/// </summary>
	public void StopPlayback()
	{
		mainAudioSource.Stop();
		ResetMarkers();
		LogDebug("Stopped playback");
	}

	/// <summary>
	///     Добавить аудио клип в очередь с отступом в миллисекундах
	/// </summary>
	/// <param name="clip">Аудио клип для воспроизведения</param>
	/// <param name="offsetMs">Отступ в миллисекундах от времени следующего маркера</param>
	/// <param name="volume">Громкость (0.0-1.0)</param>
	public void EnqueueClip(Note note)
	{
		if (note.AudioClip == null)
		{
			Debug.LogWarning("Trying to enqueue null audio clip");
			return;
		}

		clipQueue.Enqueue(note);

		// Находим следующий маркер для отображения в логах

		LogDebug($"Enqueued clip {note.name} with offset {note.ClipOffset}ms " + $"(queue size: {clipQueue.Count})");
	}

	/// <summary>
	///     Очистить всю очередь клипов
	/// </summary>
	public void ResetAll()
	{
		ResetMarkers();
		clipQueue.Clear();
		LogDebug("Cleared audio queue");
	}

	/// <summary>
	///     Создать новый маркер программно
	/// </summary>
	/// <param name="markerName">Имя нового маркера</param>
	/// <param name="timeInSeconds">Время в секундах</param>
	public void CreateMarker(string markerName, float timeInSeconds)
	{
		var newMarker = new TimeMarker
		{
			timeInSeconds = timeInSeconds,
			triggered = false
		};

		timeMarkers.Add(newMarker);
		SortMarkersByTime();

		LogDebug($"Created new marker {markerName} at {timeInSeconds} seconds");
	}

	/// <summary>
	///     Получить текущее время воспроизведения основного трека
	/// </summary>
	public float GetCurrentPlaybackTime()
	{
		if (mainAudioSource == null || mainAudioSource.clip == null)
			return 0f;

		return mainAudioSource.time;
	}

	/// <summary>
	///     Переход к определенному времени в треке
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
	///     Получить количество клипов в очереди
	/// </summary>
	public int GetQueueCount()
	{
		return clipQueue.Count;
	}

	#endregion

	#region Internal Methods

	private void ResetMarkers()
	{
		foreach (var marker in timeMarkers)
			marker.triggered = false;
	}

	private void UpdateMarkerStateBasedOnTime(float currentTime)
	{
		for (var i = 0; i < timeMarkers.Count; i++)
			if (timeMarkers[i].timeInSeconds <= currentTime)
				timeMarkers[i].triggered = true;
	}

	private void CheckTimeMarkers()
	{
		if (timeMarkers.Count == 0 || clipQueue.Count == 0)
			return;

		// Если достигли времени следующего маркера и есть клипы в очереди
		var nextMarker = timeMarkers.FirstOrDefault(n => Math.Abs(currentPlaybackTime - n.timeInSeconds) < 0.050 && !n.triggered);
		if (nextMarker != null)
		{
			nextMarker.triggered = true;
			LogDebug($"Triggered marker at {currentPlaybackTime:F2}s");
			PlayQueuedClips(nextMarker.timeInSeconds);
		}
	}

	private void PlayQueuedClips(float markerTime)
	{
		var queuedClip = clipQueue.Dequeue();

		// Вычисляем абсолютное время воспроизведения с учетом отступа
		var offsetInSeconds = queuedClip.ClipOffset / 1000.0f;
		var absolutePlayTime = markerTime + offsetInSeconds;

		var delayTime = absolutePlayTime - currentPlaybackTime;
		StartCoroutine(PlayClipWithDelay(queuedClip, delayTime));
		LogDebug($"Will play {queuedClip.AudioClip.name} after {delayTime:F2}s delay (offset {queuedClip.ClipOffset}ms)");

	}

	private IEnumerator PlayClipWithDelay(Note note, float delayInSeconds)
	{
		yield return new WaitForSeconds(delayInSeconds);

		PlayClip(note);
	}

	private void PlayClip(Note note)
	{
		if (note.AudioClip == null)
			return;

		AudioSource source;

		if (usePooling)
		{
			source = GetAvailableAudioSource();
		}
		else
		{
			var tempGO = new GameObject("TempAudio");
			source = tempGO.AddComponent<AudioSource>();
			Destroy(tempGO, note.AudioClip.length + 0.5f);
		}

		source.clip = note.AudioClip;
		source.volume = 1;
		source.Play();

		StartCoroutine(TrackAudioSourceCompletion(note, source, note.AudioClip.length));
	}

	private IEnumerator TrackAudioSourceCompletion(Note note, AudioSource source, float clipLength)
	{
		yield return new WaitForSeconds(4f);

		// Сброс источника для повторного использования
		
		NotePlay?.Invoke(note);
		if (source != null && audioSourcePool.Contains(source))
			source.clip = null;
	}

	private AudioSource GetAvailableAudioSource()
	{
		// Сначала ищем неиспользуемый источник
		foreach (var source in audioSourcePool)
			if (source != null && !source.isPlaying)
				return source;

		// Если все заняты, ищем тот, который скоро закончится
		var bestCandidate = audioSourcePool[0]; // Берем первый как запасной вариант
		var shortestRemainingTime = float.MaxValue;

		foreach (var source in audioSourcePool)
			if (source != null && source.clip != null)
			{
				var remainingTime = source.clip.length - source.time;
				if (remainingTime < shortestRemainingTime)
				{
					shortestRemainingTime = remainingTime;
					bestCandidate = source;
				}
			}

		// Останавливаем текущее воспроизведение и возвращаем этот источник
		if (bestCandidate != null)
			bestCandidate.Stop();

		return bestCandidate;
	}

	private void LogDebug(string message)
	{
		if (debugMode)
			Debug.Log($"[AudioTimelineService] {message}");
	}

	#endregion

	#region Editor Methods

	// Метод для редактора Unity - добавляет пустой маркер
	[ContextMenu("Add Empty Marker")]
	public void AddEmptyMarker()
	{
		var newMarker = new TimeMarker
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