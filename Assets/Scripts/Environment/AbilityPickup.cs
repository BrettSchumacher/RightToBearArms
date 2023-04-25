using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AbilityPickup : MonoBehaviour
{
    public List<IStateSO> stateUnlocks;
    public ParticleSystem passiveEffect;
    public GameObject grabbedEffectPrefab;

    public float grabbedEffectLength = 1f;

    BearControllerSM player;
    SpriteRenderer sr;

    bool activated = false;

    // Start is called before the first frame update
    void Start()
    {
        player = BearControllerSM.instance;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (collision == player.GetComponent<Collider2D>())
        {
            activated = true;
            player.AddMovementStates(stateUnlocks);
            StartCoroutine(PlayGrabAnim());
        }
    }

    IEnumerator PlayGrabAnim()
    {
        GameManager.FreezeGame();

        passiveEffect.Stop();

        ParticleSystem grabbedEffect = Instantiate(grabbedEffectPrefab, transform).GetComponent<ParticleSystem>();

        sr.enabled = false;
        grabbedEffect.Play();

        yield return new WaitForSecondsRealtime(grabbedEffectLength);

        grabbedEffect.Stop();

        GameManager.UnfreezeGame();
        Destroy(gameObject);
    }
}
