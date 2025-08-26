using UI.Components;
using UnityEngine;

namespace UI.Windows.Profiler
{
    public class ProfilerWindow : MonoBehaviour
    {
        private const float UPDATE_RATE = 4;
        
        [SerializeField] private GameObject _body;
        
        [SerializeField] private UIText _textFPS;
        [SerializeField] private UIText _textAllocatedRam;
        [SerializeField] private UIText _textReservedRam;
        [SerializeField] private UIText _textMonoRam;
        
        private float _deltaTime;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _body.SetActive(!_body.activeSelf);
            }
            
            if (!_body.activeSelf)
            {
                return;
            }
            
            _deltaTime += Time.unscaledDeltaTime;

            if (_deltaTime > 1f / UPDATE_RATE)
            {
                float unscaledDeltaTime = Time.unscaledDeltaTime;

                _textFPS.SetText("fps: " + Mathf.RoundToInt(1f / unscaledDeltaTime).ToString());
                
                float allocatedRam = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
                _textAllocatedRam.SetText("alloc ram: " + Mathf.RoundToInt(allocatedRam).ToString());

                float reservedRam = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576f;
                _textReservedRam.SetText("reserved ram: " + Mathf.RoundToInt(reservedRam).ToString());

                float monoRam = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1048576f;
                _textMonoRam.SetText("mono ram: " + Mathf.RoundToInt(monoRam).ToString());

                _deltaTime = 0;
            }
        }
    }
}