using UnityEngine;

namespace MathSeesaw
{
    public class SeesawResourcesManager : MonoBehaviour
    {
        static SeesawResourcesManager s_instance;

        public static SeesawResourcesManager Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = FindFirstObjectByType<SeesawResourcesManager>();
                return s_instance;
            }
            private set => s_instance = value;
        }

        [Header("Fonts")]
        [SerializeField] Font m_Font;

        [Header("Materials")]
        [SerializeField] Material m_SeesawMaterial;

        [Header("Music Clips")]
        [SerializeField] AudioClip m_MenuMusic;
        [SerializeField] AudioClip m_GameMusic;

        [Header("Sound Clips")]
        [SerializeField] AudioClip m_ButtonClickSound;
        [SerializeField] AudioClip m_PickupSound;
        [SerializeField] AudioClip m_PlaceSound;
        [SerializeField] AudioClip m_BalanceSound;
        [SerializeField] AudioClip m_CompleteSound;
        [SerializeField] AudioClip m_VictorySound;
        [SerializeField] AudioClip m_UnlockSound;

        public Font Font => m_Font;
        public Material SeesawMaterial => m_SeesawMaterial;
        public AudioClip MenuMusic => m_MenuMusic;
        public AudioClip GameMusic => m_GameMusic;
        public AudioClip ButtonClickSound => m_ButtonClickSound;
        public AudioClip PickupSound => m_PickupSound;
        public AudioClip PlaceSound => m_PlaceSound;
        public AudioClip BalanceSound => m_BalanceSound;
        public AudioClip CompleteSound => m_CompleteSound;
        public AudioClip VictorySound => m_VictorySound;
        public AudioClip UnlockSound => m_UnlockSound;

        public static Font GetFont()
        {
            var resources = Instance;
            return resources != null ? resources.Font : null;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    }
}
