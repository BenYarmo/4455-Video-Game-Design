﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCS.Characters
{
    public abstract class ProtagGroundedState : ProtagAliveState
    {
        #region variables
        protected abstract float animationTurnStrength { get; }
        protected abstract float physicsTurnStrength { get; }
        private bool jumpPressed;
        #endregion

        public override void enter(ProtagInput input)
        {
            base.enter(input);
            protag.anim.SetBool("grounded", true);
            protag.setGrounded(true);
            jumpPressed = false;
            protag.setRootMotion(true);
        }

        public override void exit(ProtagInput input)
        {
            base.exit(input);
            protag.anim.SetBool("grounded", false);
            protag.setGrounded(false);
        }

        public override void runAnimation(ProtagInput input)
        {
            base.runAnimation(input);

            float dt = Time.deltaTime * 60f;
            float v = input.v;
            float h = input.h;

            if (input.jump)
                jumpPressed = true;

            freeMovementAimation(v, h, input.totalMotionMag, dt);
        }

        public override bool runLogic(ProtagInput input)
        {
            if (base.runLogic(input))
                return true;

            protag.checkGroundGrounded();

            protag.rb.AddForce(-Vector3.ProjectOnPlane(Physics.gravity, protag.getGroundNormal()));

            if (!protag.getGrounded())
            {
                protag.newState<ProtagFallingState>();
                return true;
            }
            else if (jumpPressed)
            {
                protag.newState<ProtagJumpingState>();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void freeMovementAimation(float v, float h, float mag, float dt)
        {

            Vector3 move = InputManager.calculateMove(v, h);

            //rotate game object
            if (move != Vector3.zero)
            {
                Quaternion goalRot = Quaternion.LookRotation(move, Vector3.up);
                protag.anim.transform.rotation = Quaternion.Slerp(protag.anim.transform.localRotation, goalRot, physicsTurnStrength * dt * move.magnitude);
            }

            //set forward motion
            float scale = (Mathf.Abs(protag.anim.GetFloat("vertical")) < Mathf.Abs(mag)) ? 1f : 4f; // speed up gradually, slow down quickly
            float targetV = mag;
            float nextV = Mathf.Lerp(protag.anim.GetFloat("vertical"), targetV, dt * .05f * scale);
            protag.anim.SetFloat("vertical", nextV);

            //set turn
            float turnAmount = mag * Vector3.Angle(protag.anim.transform.forward, move) / 180;
            float turnDir = Utility.AngleDir(protag.anim.transform.forward, move, Vector3.up);
            float targetH = turnDir * turnAmount * animationTurnStrength;
            float nextH = Mathf.Lerp(protag.anim.GetFloat("horizontal"), targetH, dt * .1f);
            protag.anim.SetFloat("horizontal", nextH);

            protag.anim.SetFloat("movementMagnitude", mag);
        }
    }
}
