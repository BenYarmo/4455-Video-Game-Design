﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCS.Characters
{
    public class Protag : Character<ProtagState, ProtagInput>
    {
        #region variables

        [Header("Hitboxes")]
        public Collider[] hurtBoxes;
        public Collider[] hitBoxes;

        [Header("Movement Settings")]
        public float jumpStrength = 10;
        public float aerialMovementStrength;
        public float aerialDrag;
        
        [SerializeField]
        private AnimationCurve compressionCurve;

        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public CapsuleCollider col;
        [HideInInspector]
        public Animator anim;

        private PlayerCameraController cam;
        private int selfMask;
        private bool vuln;
        private bool grounded;
        private Vector3 groundNormal;
        private bool aerial;

        #endregion

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            anim = GetComponentInChildren<Animator>();
            cam = GameObject.FindObjectOfType<PlayerCameraController>();
            
            anim.applyRootMotion = true;
            vuln = true;
            grounded = true;
            aerial = false;

            selfMask = ~ LayerMask.GetMask("Player");

            newState<ProtagLocomotionState>();

            // enable hurtboxes
            foreach (Collider hb in hurtBoxes)
                hb.enabled = true;

            // disable hitboxes
            foreach (Collider hb in hitBoxes)
                hb.enabled = false;
        }

        public override void readInput ()
        {
            input.v = InputManager.getMotionForward();
            input.h = InputManager.getMotionHorizontal();
            input.totalMotionMag = InputManager.getTotalMotionMag();
            input.jump = InputManager.getJump();
        }

        public bool checkGroundGrounded()
        {
            Vector3 pos = transform.position + (Vector3.up * 0.5f);
            Vector3 dir = (Vector3.down * 0.8f);
            RaycastHit groundCheck;
            if (Physics.Raycast(pos, dir, out groundCheck, 0.8f, selfMask))
            {
                Debug.DrawRay(pos, dir, Color.green);
                grounded = true;
                groundNormal = groundCheck.normal;
                return true;
            }
            else
            {
                Debug.DrawRay(pos, dir, Color.red);
                grounded = false;
                groundNormal = Vector3.up;
                return false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (aerial)
            {
                Vector3 pos = transform.position + (Vector3.up * 0.5f);
                Vector3 dir = (Vector3.down * 0.8f);
                RaycastHit groundCheck;
                if (Physics.Raycast(pos, dir, out groundCheck, 0.8f, selfMask))
                {
                    Debug.DrawRay(pos, dir, Color.green);
                    grounded = true;
                    groundNormal = groundCheck.normal;
                }
                else
                {
                    Debug.DrawRay(pos, dir, Color.red);
                    grounded = false;
                    groundNormal = Vector3.up;
                }
            }
        }

        public float sampleCompressionCurve(float x) { return compressionCurve.Evaluate(x); }

        public bool getGrounded() { return grounded; }

        public bool getVulnerable() { return vuln; }

        public Vector3 getGroundNormal() { return groundNormal; }

        public void setGrounded(bool value) { grounded = value; }

        public void setVulnerable(bool value) { vuln = value; }

        public void setRootMotion(bool value) { anim.applyRootMotion = value; }

        public void setAerial(bool value) { aerial = value; }
    }

    public class ProtagInput : CharacterInput
    {
        public float v;
        public float h;
        public float totalMotionMag;

        public bool jump;
    }
}
