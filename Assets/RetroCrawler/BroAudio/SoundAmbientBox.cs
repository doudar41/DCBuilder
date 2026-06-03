using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SoundAmbientBox : MonoBehaviour
{
    [SerializeField] SoundID mainAmbient = default;
    [SerializeField] List<AudioClip> randomAmbientSounds = new List<AudioClip>();
    [SerializeField] Vector2 randomSoundInterval = new Vector2(15f, 25f);
    [SerializeField]AudioSource randomAudioSource;
    BoxCollider boxCollider;
    bool playerInside = false;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(mainAmbient !=default) BroAudio.Play(mainAmbient);
            playerInside = true;
            StartCoroutine(PlayRandomSounds());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }


    IEnumerator PlayRandomSounds()
    {
        while (playerInside)
        {
            if (randomAmbientSounds.Count > 0)
            {
                Vector3 boxCenter = boxCollider.bounds.center; // Ensure the bounds are updated to the current position
                Vector3 extents = boxCollider.bounds.extents;
                float randomX = Random.Range(boxCenter.x - extents.x, boxCenter.x + extents.x);
                float randomZ = Random.Range(boxCenter.z - extents.z, boxCenter.z + extents.z);
                GameObject soundSource = Instantiate(new GameObject("AmbientSound"), new Vector3(randomX, 2.5f,randomZ), Quaternion.identity);
                int randomIndex = Random.Range(0, randomAmbientSounds.Count);
                randomAudioSource.transform.parent = soundSource.transform;
                //BroAudio.Play(randomAmbientSounds[randomIndex], soundSource.transform);
                randomAudioSource.clip = randomAmbientSounds[randomIndex];
                randomAudioSource.Play();
                yield return new WaitForSeconds(Random.Range(randomSoundInterval.x, randomSoundInterval.y)); // Adjust the range as needed
                randomAudioSource.transform.parent = null;
                DestroyImmediate(soundSource);
            }
        }
    }


}
