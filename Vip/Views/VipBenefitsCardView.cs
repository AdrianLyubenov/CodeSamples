using TMPro;
using UnityEngine;

namespace KingOfDestiny.Vip.Views
{
    public class VipBenefitsCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Description;
        [SerializeField] private GameObject m_NewTag;
        [SerializeField] private GameObject m_LockIcon;
        [SerializeField] private GameObject m_BulletPoint;

        public void UpdateView(string description, bool isNew, bool isLocked)
        {
            if (isLocked)
            {
                m_LockIcon.SetActive(true);
                m_NewTag.SetActive(false);
                m_BulletPoint.SetActive(false);
            }
            else
            {
                m_LockIcon.SetActive(false);
                m_NewTag.SetActive(isNew);
                m_BulletPoint.SetActive(true);
            }

            m_Description.text = description;
        }
    }
}