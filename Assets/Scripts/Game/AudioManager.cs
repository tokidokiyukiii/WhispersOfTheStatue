using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static Inventory;

public class AudioManager : MonoBehaviour
{
    public AudioClip eatSound;
    public AudioClip cookedSound;
    public AudioClip warningSound;
    public AudioClip resultSound;
    public AudioClip celebrateSound;
    public AudioClip heartbeatSound;
    public AudioClip acceptSound;
    public AudioClip gameOverSound;
    public AudioClip gamePassSound;
    public AudioClip reviveSound;
    public AudioClip damageSound;
    public AudioClip shootSound;
    public AudioClip attackSound;
    public AudioClip enemySound;
    public AudioClip collectSound;
    public AudioClip purchaseSound;
    public AudioClip chestSound;
    public AudioClip unlockSound;
    public AudioClip statueSound;
    public AudioClip swapSound;
    public AudioClip mapSound;

    public AudioSource uiAudioSource;
    private Coroutine _interactionCoroutine;

    public void StopAudio()
    {
        if (uiAudioSource?.isPlaying == true) uiAudioSource.Stop();
    }
    public void PlayEatFeedback(FoodType foodType)
    {
        if (eatSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(eatSound);
        }
    }
    public void PlayCookedSound()
    {
        if (cookedSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(cookedSound);
        }
    }

    public void PlayWarningSound()
    {
        if (warningSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(warningSound);
        }
    }

    public void PlayResultSound()
    {
        if (resultSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(resultSound);
        }
    }

    public void PlayCelebrateSound()
    {
        if (celebrateSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(celebrateSound);
        }
    }

    public void PlayHeartbeatSound()
    {
        if (heartbeatSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(heartbeatSound);
        }
    }
    public void PlayAcceptSound()
    {
        if (acceptSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(acceptSound);
        }
    }
    public void PlayGameOverSound()
    {
        if (gameOverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(gameOverSound);
        }
    }
    public void PlayGamePassSound()
    {
        if (gamePassSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(gamePassSound);
        }
    }
    public void PlayReviveSound()
    {
        if (reviveSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(reviveSound);
        }
    }
    public void PlayDamageSound()
    {
        if (damageSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(damageSound);
        }
    }
    public void PlayShootSound()
    {
        if (shootSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(shootSound);
        }
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(attackSound);
        }
    }
    public void PlayEnemySound()
    {
        if (enemySound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(enemySound);
        }
    }
    public void PlayCollectSound()
    {
        if (collectSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(collectSound);
        }
    }
    public void PlayPurchaseSound()
    {
        if (purchaseSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(purchaseSound);
        }  
    }
    public void PlayChestSound()
    {
        if (chestSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(chestSound);
        }
    }
    public void PlayUnlockSound()
    {
        if (unlockSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(unlockSound);
        }
    }

    public void PlayStatueSound()
    {
        if (statueSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(statueSound);
        }
    }
    public void PlaySwapSound()
    {
        if (swapSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(swapSound);
        }
    }
    public void PlayMapSound()
    {
        if (mapSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(mapSound);
        }
    }
}
