using UnityEngine;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Events;

public class MusicManager : MonoBehaviour
{
    public static MusicManager me;

    [SerializeField]
    public FMODUnity.EventReference music;

    [StructLayout(LayoutKind.Sequential)]
    public class TimelineInfo
    {
        public int currentBeat = 0;
        public int currentBar = 0;
        public float currentTempo = 0;
        public int currentPosition = 0;
        public float songLength = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }

    public TimelineInfo timelineInfo = null;

    private GCHandle timelineHandle;

    private FMOD.Studio.EVENT_CALLBACK beatCallback;
    private FMOD.Studio.EventDescription descriptionCallback;

    public FMOD.Studio.EventInstance musicPlayEvent;

    private void Awake()
    {
        me = this;

        musicPlayEvent = RuntimeManager.CreateInstance(music);
        musicPlayEvent.start();
    }

    private void Start()
    {
        timelineInfo = new TimelineInfo();
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);

        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        musicPlayEvent.setUserData(GCHandle.ToIntPtr(timelineHandle));
        musicPlayEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        musicPlayEvent.getDescription(out descriptionCallback);
        descriptionCallback.getLength(out int length);

        timelineInfo.songLength = length;
    }

    private void Update()
    {
        musicPlayEvent.getTimelinePosition(out timelineInfo.currentPosition);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Box(
            $"Current Beat = {timelineInfo.currentBeat}" +
            $"\n Current Bar = {timelineInfo.currentBar}" +
            $"\n Current Tempo = {timelineInfo.currentTempo}" +
            $"\n Current Position = {timelineInfo.currentPosition}" +
            $"\n Song Length = {timelineInfo.songLength}" +
            $"\n Last Marker = {(string)timelineInfo.lastMarker}");
    }
#endif

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero) //System(IntPtr)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentBeat = parameter.beat;
                        timelineInfo.currentBar = parameter.bar;
                        timelineInfo.currentTempo = parameter.tempo;
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }

    void OnDestroy()
    {
        musicPlayEvent.setUserData(IntPtr.Zero);
        musicPlayEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicPlayEvent.release();
        timelineHandle.Free();
    }
}
