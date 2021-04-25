using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [Serializable]
    public class UIEnergy : MonoBehaviour
    {
        public Image m_ProgressBar;

        private void FixedUpdate()
        {
            m_ProgressBar.fillAmount = Character.Instance.m_Energy / Character.Instance.m_EnergyMax;
        }
    }
}