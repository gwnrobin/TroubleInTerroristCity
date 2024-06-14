using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UI_PlayerHealth : MonoBehaviour
{
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private Volume _volume;

    private float intensity = 0;
    
    public void BindPlayer(Player player)
    {
        if (player != null)
        {
            _progressBar.SetValue(player.Health.Val);
            player.Health.AddChangeListener(health => _progressBar.SetValue(health));
            player.Health.AddChangeListener(_ => GetHitEffect());
        }
    }

    private void GetHitEffect()
    {
        StartCoroutine(TakeDamage());
    }

    private IEnumerator TakeDamage()
    {
        if (_volume.profile.TryGet(out Vignette vignette))
        {
            intensity = .4f;
            
            vignette.intensity.value = .4f;

            yield return new WaitForSeconds(0.4f);

            while (intensity > 0)
            {
                intensity -= .01f;

                if (intensity < 0) intensity = 0;

                vignette.intensity.value = intensity;

                yield return new WaitForSeconds(.1f);
            }

            vignette.intensity.value = 0;
            yield break;
        }
    }
}
