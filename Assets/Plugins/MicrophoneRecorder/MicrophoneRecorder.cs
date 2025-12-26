using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MicrophoneRecorder : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TextMeshProUGUI textRecordingTime;
    [SerializeField] private Transform groupLineContainer;
    private const int SAMPLE_RATE = 16000;
    private Action onStartRecording, onRecording, onEndRecording;
    private Coroutine recordingCoroutine;
    private AudioClip audioClip;
    private string micDevice;
    private int recordingDuration = 30;
    private bool isRecording;
    private float startTime;
    private byte[] audioBytes;
    private Stopwatch stopWatch = new Stopwatch();

    public void OnClickClose()
    {
        ResetRecordingState();
        gameObject.SetActive(false);
    }
   
    public void test(float[] data)
    {
        audioClip.SetData(data, 0);
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    
    [ContextMenu("test")]
    public void t()
    {
        byte[] returnedBytes;
        using (MemoryStream output = new())
        {
            using (GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(audioBytes, 0, audioBytes.Length);
            }
            returnedBytes = output.ToArray();
        }
        byte[] reversedBytes;
        using (MemoryStream input = new(returnedBytes))
        {
            using (GZipStream gzip = new(input, System.IO.Compression.CompressionMode.Decompress))
            {
                using (MemoryStream output = new())
                {
                    gzip.CopyTo(output);
                    reversedBytes = output.ToArray();
                }
            }
        }
        int totalSamples = reversedBytes.Length / 4;
        float[] samples = new float[totalSamples];
        Buffer.BlockCopy(reversedBytes, 0, samples, 0, reversedBytes.Length);
        audioClip.SetData(samples, 0);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void SetData(int duration, Action onStartCb = null, Action onRecordingCb = null, Action onEndCb = null)
    {
        if (duration <= 0) return;
        recordingDuration = duration;
        onStartRecording = onStartCb;
        onRecording = onRecordingCb;
        onEndRecording = onEndCb;

    }
    public bool IsDeviceHasMicro() { return Microphone.devices.Length > 0; }
    public byte[] GetBytes() { return audioBytes; }
    public void PlayRecord(float[] samples = null)
    {
        if (samples != null) audioClip.SetData(samples, 0);
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    public void StartRecording()
    {
        if (!IsDeviceHasMicro())
        {
            micDevice = Microphone.devices[0];
            return;
        }
        stopWatch.Reset();
        stopWatch.Start();
        startTime = Time.realtimeSinceStartup;
        recordingCoroutine = StartCoroutine(Recording());
    }
    public void StopRecording()
    {
        if (!isRecording) return;
        EndRecording();
        if (recordingCoroutine != null) StopCoroutine(recordingCoroutine);
        onEndRecording?.Invoke();
    }
    private void EndRecording()
    {
        Microphone.End(micDevice);
        audioClip = TrimAudioClip();
        textRecordingTime.gameObject.SetActive(false);

    }
    public void ResetRecordingState()
    {
        // Dừng mọi ghi âm đang diễn ra
        if (isRecording || Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
        }

        // Dừng coroutine nếu đang chạy
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }
        // _MicAC = null;
        audioBytes = null;
        isRecording = false;
        startTime = 0f;
        if (textRecordingTime != null)
        {
            textRecordingTime.text = "00:00:00";
            textRecordingTime.gameObject.SetActive(false);
        }
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
        // _OnStartRecordingCb = null;
        //  _OnRecordingCb = null;
        //  _OnEndRecordingCb = null;
        if (stopWatch != null)
        {
            stopWatch.Reset();
        }
    }
    private AudioClip TrimAudioClip()
    {
        float recordedLength = Time.realtimeSinceStartup - startTime;
        int samplesLength = (int)(audioClip.frequency * recordedLength);
        float[] samples = new float[samplesLength];
        audioClip.GetData(samples, 0);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= 5f;
            samples[i] = Mathf.Clamp(samples[i], -1f, 1f); // bắt buộc
        }
        audioBytes = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, audioBytes, 0, audioBytes.Length);
        UnityEngine.Debug.Log("xem frequence may" + audioClip.frequency + "  " + AudioSettings.outputSampleRate);
        AudioClip trimmedAC = AudioClip.Create(audioClip.name, samplesLength, audioClip.channels, SAMPLE_RATE, false);
        trimmedAC.SetData(samples, 0);
        return trimmedAC;
    }

    private IEnumerator Recording()
    {
        isRecording = true;
        audioClip = Microphone.Start(micDevice, false, recordingDuration, SAMPLE_RATE);
        textRecordingTime.gameObject.SetActive(true);
        textRecordingTime.text = "00:00:00";
        onStartRecording?.Invoke();
        int countDownTime = recordingDuration;
        while (countDownTime > 0)
        {

            countDownTime -= 1;
            onRecording?.Invoke();
            int time = recordingDuration - countDownTime;
            if (time > 0) textRecordingTime.text = string.Format("{0:00}:{1:00}:{2:00}",
                Mathf.FloorToInt(time / 60 / 60), Mathf.FloorToInt(time / 60 % 60), Mathf.FloorToInt(time % 60));
            yield return new WaitForSecondsRealtime(1f);
        }
        EndRecording();
        if (onEndRecording == null) OnClickClose();
        else onEndRecording.Invoke();
        isRecording = false;
    }

    private void Start()
    {
        if (IsDeviceHasMicro()) micDevice = Microphone.devices[0];
    }

    private void Update()
    {
        if (isRecording)
        {
            DrawLiveWaveform();
        }
    }
    private void DrawLiveWaveform()
    {
        if (audioClip == null || groupLineContainer == null) return;

        int micPos = Microphone.GetPosition(micDevice);
        if (micPos < 1024) return;

        const int sampleLength = 1024;
        float[] samples = new float[sampleLength];

        int startPos = micPos - sampleLength;
        if (startPos < 0) return;

        audioClip.GetData(samples, startPos);

        int barCount = groupLineContainer.transform.childCount;
        int segmentLength = sampleLength / barCount;

        for (int i = 0; i < barCount; i++)
        {
            float max = 0f;

            for (int j = 0; j < segmentLength; j++)
            {
                int idx = i * segmentLength + j;
                if (idx >= samples.Length) break;
                max = Mathf.Max(max, Mathf.Abs(samples[idx]));
            }

            float normalized = Mathf.Clamp01(max * 20f); // khuếch đại
            float scaleY = Mathf.Lerp(0.1f, 1f, normalized); // đảm bảo vạch tối thiểu

            Transform bar = groupLineContainer.transform.GetChild(i);
            if (bar != null)
            {
                bar.localScale = new Vector3(1f, scaleY, 1f);

                Image img = bar.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.green;
                }
            }
        }
    }

}
