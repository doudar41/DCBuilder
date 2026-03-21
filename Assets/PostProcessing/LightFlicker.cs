
using UnityEngine;

namespace OldCode
{
    public class LightFlicker : MonoBehaviour
    {
        // Variables
        private new Light light;

        // How many steps we want for the smoothing queue
        //   less = faster flicker
        //   more = smoother variation
        public int ShadingSteps = 32;
        public float TargetIntensity = 4.0f;
        public float MinimumIntensityDifference = 0.15f;
        public float MaximumIntensityDifference = 1.5f;

        [SerializeField] bool isDungeon = false;
        // Target framerate for flicker
        public float FrameRate = 12.0f;

        // Smoothing array
        private float[] smoothing;

        bool isNight = true;

        [SerializeField] bool nightDepended = true;

        private void Awake()
        {
           if(nightDepended) GameInstance.progress += DayNightChange;
            else isNight = true;
        }

        private void OnDestroy()
        {
            if (nightDepended) GameInstance.progress -= DayNightChange;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Get our light
            light = this.GetComponent<Light>();

            // Initiliaze the smoothing array
            smoothing = new float[ShadingSteps];

            // Initialize the array.
            for (int i = 0; i < smoothing.Length; i++)
            {
                smoothing[i] = TargetIntensity; // 0.0f
            }

            // Start flicker and time it to a specific framerate
            float timeDelay = 1.0f / FrameRate;
            InvokeRepeating("FlickerAway", 0.0f, timeDelay);
        }


        // Call the flicker separately since we want parameters
        void FlickerAway()
        {
            if (isDungeon) { FlickerLight(ShadingSteps, 1); return; }

            if (isNight) 
            {
                FlickerLight(ShadingSteps, 1);
            }
            else
            {
                light.intensity = 0;
            }
        }


        /*
         * Flicker a Unity Light source
         * 
         * Generates a list of Light.intensity levels based on parameters
         * and cycles through it to generate a flickering light effect
         */
        void FlickerLight(int steps, float intensity)
        {
            // Shift values in the table so that the new one is at the end and the older one is deleted
            for (int i = 1; i < smoothing.Length; i++)
            {
                smoothing[i - 1] = smoothing[i];
                intensity += smoothing[i - 1];
            }

            // Calculate a new flicker range based on the known limits
            float newMinimum = UnityEngine.Random.Range(TargetIntensity - MaximumIntensityDifference, TargetIntensity);
            float newMaximum = UnityEngine.Random.Range(TargetIntensity, TargetIntensity + MaximumIntensityDifference);

            // Add the new value at the end of the array.
            smoothing[smoothing.Length - 1] = UnityEngine.Random.Range(newMinimum, newMaximum);
            intensity += smoothing[smoothing.Length - 1];

            // Compute the average of the array and assign it to the light intensity
            light.intensity = intensity / smoothing.Length;
        }


        void DayNightChange(int count)
        {

            //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());
            if (GameInstance.GetNormalTime()[1]%24 >= 6 && GameInstance.GetNormalTime()[1] % 24 < 19)
            {
                isNight = false;
            }
            else
            {
                isNight = true;
            }
        }


    }



}