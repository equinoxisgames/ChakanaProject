using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Enemy : CharactersBehaviour
{
    [SerializeField] protected GameObject deathFX;
    [SerializeField] protected Transform groundDetector;
    [SerializeField] protected Transform wallDetector;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask wallLayer;
    protected Animator anim;
    [SerializeField] protected GameObject healthBar;
    protected EnemyHealthBar bar;

    protected virtual float OrientacionDeteccionPlayer(float playerPositionX)
    {
        if (playerPositionX < transform.position.x) return -1;
        else if (playerPositionX > transform.position.x) return 1;

        return playerPositionX;
    }

    protected override IEnumerator cooldownRecibirDanio(int direccion, float fuerzaRecoil)
    {
        Recoil(direccion, fuerzaRecoil);
        if (vida <= 0)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);
        playable = true;
    }
}
