using UnityEngine;

public class Boss : MonoBehaviour {
    public float health = 100;
    Animator animator;

    private bool isDead = false;

    private void Start() {
        animator = GetComponent<Animator>();
    }
    public void characterHitDamage(float takeDamage) {
        if (isDead) return;

        health -= takeDamage;

        if (health <= 0) {
            animator.SetBool("Die", true);
            characterDie();
        }
    }

    void characterDie() {
        
    }

}
