using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct AudioSFX
{
    public AudioClip clip;
    public float volume;

}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgMusic;
    public AudioSource sfxMain;
    public GameObject sfxPrefab;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate AudioManagers, deleting one");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void PlaySFX(AudioSFX sfx)
    {
        if (!sfxMain.isPlaying)
        {
            sfxMain.clip = sfx.clip;
            sfxMain.volume = sfx.volume;
            sfxMain.Play();
        }
        else
        {
            AudioSource tempSource = Instantiate(sfxPrefab, transform).GetComponent<AudioSource>();

            tempSource.clip = sfx.clip;
            tempSource.volume = sfx.volume;
            tempSource.Play();
            StartCoroutine(DestroySource(tempSource));
        }
    }

    public static AudioSource GetBGMusicSource() { return instance.bgMusic; }
    
    public static void PlaySound(AudioSFX sfx)
    {
        instance.PlaySFX(sfx);
    }

    IEnumerator DestroySource(AudioSource source)
    {
        if (source.isPlaying)
        {
            yield return new WaitForSeconds(source.clip.length - source.time);
        }
        Destroy(source.gameObject);
    }
}
