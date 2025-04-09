namespace ArionDigital
{
    using UnityEngine;

    public class CrashCrate : MonoBehaviour
    {
        [Header("Whole Create")]
        public MeshRenderer wholeCrate;
        public BoxCollider boxCollider;
        [Header("Fractured Create")]
        public GameObject fracturedCrate;
        [Header("Audio")]
        public AudioSource crashAudioClip;

        [Header("Destory Time")]
        public float timeToDestory;

        private void OnTriggerEnter(Collider other)
        {
            wholeCrate.enabled = false;
            boxCollider.enabled = false;
            fracturedCrate.SetActive(true);
            crashAudioClip.Play();
            Invoke("DestroySelf", timeToDestory);
        
        }

        [ContextMenu("Test")]
        public void Test()
        {
            wholeCrate.enabled = false;
            boxCollider.enabled = false;
            fracturedCrate.SetActive(true);
        }

        void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}