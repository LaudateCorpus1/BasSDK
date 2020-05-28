﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using EasyButtons;
#endif

namespace ThunderRoad
{
    public class WristStats : MonoBehaviour
    {
        [HideInInspector]
        public float showDistance = 0.5f;
        [HideInInspector]
        public float showAngle = 30f;
        [HideInInspector]
        public Vector3 localPosition;
        [HideInInspector]
        public Vector3 localRotation;

        protected bool isShown = true;

#if ODIN_INSPECTOR
        [ValueDropdown("GetAllEffectID")]
#endif
        public string lifeEffectId;
        public Transform lifeEffectParent;
#if ODIN_INSPECTOR
        [ValueDropdown("GetAllEffectID")]
#endif
        public string manaEffectId;
        public Transform manaEffectParent;
#if ODIN_INSPECTOR
        [ValueDropdown("GetAllEffectID")]
#endif
        public string focusEffectId;
        public Transform focusEffectParent;

        public bool initialized = false;

#if ProjectCore
        public List<ValueDropdownItem<string>> GetAllEffectID()
        {
            return Catalog.GetDropdownAllID(Catalog.Category.Effect);
        }

        private EffectData lifeEffectData;
        private EffectData manaEffectData;
        private EffectData focusEffectData;

        private EffectInstance lifeEffectInstance;
        private EffectInstance manaEffectInstance;
        private EffectInstance focusEffectInstance;
        protected Creature creature;


        public void Init()
        {
            creature = this.GetComponentInParent<Creature>();
            StartCoroutine(DelayedInit());
        }

        protected IEnumerator DelayedInit()
        {
            BodyHand bodyHand = this.GetComponentInParent<BodyHand>();
            while (!bodyHand.body.initialized) yield return null;
            Transform armTwist = bodyHand.lowerArmBone.Find(bodyHand.side == Side.Left ? "LeftForeArmTwist" : "RightForeArmTwist");
            this.transform.SetParent(armTwist, true);
            this.transform.localPosition = localPosition;
            this.transform.localRotation = Quaternion.Euler(localRotation);

            lifeEffectData = Catalog.GetData<EffectData>(lifeEffectId);
            manaEffectData = Catalog.GetData<EffectData>(manaEffectId);
            focusEffectData = Catalog.GetData<EffectData>(focusEffectId);

            initialized = true;
        }

        void Update()
        {
            if (!creature.centerEyes) return;
            if (!initialized) return;

            float eyesDistance = Vector3.Distance(creature.centerEyes.position, this.transform.position);
            float eyesAngle = Vector3.Angle(-creature.centerEyes.forward, this.transform.forward);

            if (eyesDistance < showDistance && eyesAngle < showAngle)
            {
                if (!isShown)
                {
                    Show(true);
                    isShown = true;
                }
            }
            else if (isShown)
            {
                Show(false);
                isShown = false;
            }

            if (isShown)
            {
                if (creature.health)
                {
                    lifeEffectInstance.SetIntensity(creature.health.currentHealth / creature.health.maxHealth);
                }
                if (creature.mana)
                {
                    manaEffectInstance.SetIntensity(creature.mana.currentMana / creature.mana.maxMana);
                    if (creature.body.player)
                    {
                        // Time rune vfx is separated in 3 bars
                        // 0 -> 0
                        // 0.25 -> 1
                        // 0.50 -> 2
                        // 0.75 -> 3
                        focusEffectInstance.SetIntensity(Mathf.Lerp(0, 0.75f, creature.mana.currentFocus / creature.mana.maxFocus));
                    }
                }
            }
        }

        [Button]
        void Show(bool active)
        {
            if (active)
            {
                lifeEffectInstance = lifeEffectData.Spawn(lifeEffectParent);
                manaEffectInstance = manaEffectData.Spawn(manaEffectParent);
                focusEffectInstance = focusEffectData.Spawn(focusEffectParent);

                lifeEffectInstance.Play();
                manaEffectInstance.Play();
                focusEffectInstance.Play();
            }
            else
            {
                if (lifeEffectInstance != null) lifeEffectInstance.Stop();
                if (manaEffectInstance != null) manaEffectInstance.Stop();
                if (focusEffectInstance != null) focusEffectInstance.Stop();
            }
        }
#endif
    }
}